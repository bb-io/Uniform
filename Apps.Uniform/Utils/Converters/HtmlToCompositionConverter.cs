using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;
using Apps.Uniform.Extensions;
using Apps.Uniform.Utils.Converters.Models;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Uniform.Utils.Converters;

public static class HtmlToCompositionConverter
{
    public static (string compositionId, string locale) ExtractMetadata(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return (GetMetaContent(doc, "blackbird-composition-id"), GetMetaContent(doc, "blackbird-locale"));
    }

    private static string GetMetaContent(HtmlDocument doc, string name) =>
        doc.DocumentNode.SelectSingleNode($"//meta[@name='{name}']")?.GetAttributeValue("content", "") ?? "";

    public static Dictionary<string, List<PatternTranslation>> UpdateCompositionFromHtml(
        string html, 
        JObject compositionData,
        string targetLocale)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var sourceLocale = GetMetaContent(doc, "blackbird-locale");

        var body = doc.DocumentNode.SelectSingleNode("//body") ?? 
                   throw new PluginMisconfigurationException("Invalid HTML: Body element not found");

        if (!compositionData.HasValues)
        {
            var originalJsonEncoded = body.GetAttributeValue("data-original-json", "");
            if (string.IsNullOrEmpty(originalJsonEncoded))
                throw new PluginMisconfigurationException("Invalid HTML: Original JSON not found in body attribute");

            var originalJson = HttpUtility.HtmlDecode(originalJsonEncoded);
            var originalComposition = JObject.Parse(originalJson);

            foreach (var prop in originalComposition.Properties())
                compositionData[prop.Name] = prop.Value.DeepClone();
        }

        var patternTranslations = new Dictionary<string, List<PatternTranslation>>();

        var parameterDivs = doc.DocumentNode.SelectNodes("//div[@data-json-path]");
        if (parameterDivs != null)
        {
            foreach (var parameterDiv in parameterDivs)
            {
                var jsonPath = parameterDiv.GetAttributeValue("data-json-path", "");
                if (string.IsNullOrEmpty(jsonPath))
                    continue;

                var pathParts = ParseJsonPath(jsonPath);

                var (patternId, relativeParts) = FindOwningPattern(compositionData, pathParts);
                if (patternId != null)
                {
                    if (!patternTranslations.TryGetValue(patternId, out var translations))
                        patternTranslations[patternId] = translations = new List<PatternTranslation>();

                    translations.Add(new PatternTranslation(relativeParts, parameterDiv));
                    continue;
                }

                ApplyTranslation(compositionData, pathParts, parameterDiv, sourceLocale, targetLocale);
            }
        }

        EnsureTargetLocale(compositionData, targetLocale);

        return patternTranslations;
    }
    
    public static void UpdatePatternFromTranslations(
        JObject patternData,
        IEnumerable<PatternTranslation> translations,
        string sourceLocale,
        string targetLocale)
    {
        foreach (var translation in translations)
            ApplyTranslation(patternData, translation.PathParts, translation.ParameterDiv, sourceLocale, targetLocale);

        if (patternData["_locales"] is JArray)
            EnsureTargetLocale(patternData, targetLocale);
    }
    
    private static void ApplyTranslation(
        JObject root,
        List<string> pathParts,
        HtmlNode parameterDiv,
        string sourceLocale,
        string targetLocale)
    {
        var parameterObject = ResolveParameterObject(root, pathParts);
        if (parameterObject == null)
            return; // Path not found, skip

        var parameterType = parameterObject["type"]?.ToString();
        if (string.IsNullOrEmpty(parameterType))
            parameterType = parameterDiv.GetAttributeValue("data-parameter-type", "");

        JToken translatedValue;
        if (string.Equals(parameterType, "richText", StringComparison.OrdinalIgnoreCase))
            translatedValue = new HtmlToRichTextConverter().ToRichText(parameterDiv.InnerHtml);
        else
        {
            var textNode = parameterDiv.SelectSingleNode(".//h1 | .//h2 | .//h3 | .//p");
            if (textNode == null)
                return;

            translatedValue = HttpUtility.HtmlDecode(textNode.InnerText);
        }

        SetLocaleValue(parameterObject, sourceLocale, targetLocale, translatedValue);
    }
    
    private static void EnsureTargetLocale(JObject root, string targetLocale)
    {
        if (root["_locales"] is not JArray locales)
        {
            locales = new JArray();
            root["_locales"] = locales;
        }

        if (locales.All(l => l.ToString() != targetLocale))
            locales.Add(targetLocale);
    }
    
    private static (string? PatternId, List<string> RelativeParts) FindOwningPattern(JObject root, List<string> pathParts)
    {
        string? patternId = null;
        var relativeStart = 0;
        JToken? current = root;

        for (var i = 0; i < pathParts.Count - 2 && current != null; i++)
        {
            current = current.Step(pathParts[i]);

            if (current is not JObject node || node["_pattern"]?.ToString() is not { Length: > 0 } id) 
                continue;
            
            patternId = id;
            relativeStart = i + 1;
        }

        return (patternId, pathParts.Skip(relativeStart).ToList());
    }
    
    private static JObject? ResolveParameterObject(JObject root, List<string> pathParts)
    {
        JToken? current = root;

        for (var i = 0; i < pathParts.Count - 2 && current != null; i++)
            current = current.Step(pathParts[i]);

        return current as JObject;
    }

    private static void SetLocaleValue(JObject parameterObject, string sourceLocale, string targetLocale, JToken value)
    {
        if (parameterObject["locales"] is not JObject locales)
        {
            locales = new JObject();

            if (parameterObject["value"] is { } sourceValue && !string.IsNullOrEmpty(sourceLocale))
            {
                locales[sourceLocale] = sourceValue;
            }

            parameterObject.Remove("value");
            parameterObject["locales"] = locales;
        }

        locales[targetLocale] = value;
    }

    private static List<string> ParseJsonPath(string jsonPath)
    {
        var pathParts = new List<string>();
        var currentPart = "";

        for (int i = 0; i < jsonPath.Length; i++)
        {
            var c = jsonPath[i];
            if (c == '.')
            {
                if (!string.IsNullOrEmpty(currentPart))
                {
                    pathParts.Add(currentPart);
                    currentPart = "";
                }
            }
            else if (c == '[')
            {
                if (!string.IsNullOrEmpty(currentPart))
                {
                    pathParts.Add(currentPart);
                }
                var endIndex = jsonPath.IndexOf(']', i);
                pathParts.Add(jsonPath.Substring(i, endIndex - i + 1));
                i = endIndex;
                currentPart = "";
            }
            else
            {
                currentPart += c;
            }
        }

        if (!string.IsNullOrEmpty(currentPart))
        {
            pathParts.Add(currentPart);
        }

        return pathParts;
    }
}
