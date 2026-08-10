using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class HtmlToCompositionConverter
{
    public static (string compositionId, string locale) ExtractMetadata(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var compositionIdMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-composition-id']");
        var localeMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-locale']");

        var compositionId = compositionIdMeta?.GetAttributeValue("content", "") ?? "";
        var locale = localeMeta?.GetAttributeValue("content", "") ?? "";

        return (compositionId, locale);
    }

    public void UpdateCompositionFromHtml(string html, JObject compositionData, string targetLocale)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var body = doc.DocumentNode.SelectSingleNode("//body");
        if (body == null)
        {
            throw new Exception("Invalid HTML: Body element not found");
        }

        if (!compositionData.HasValues)
        {
            var originalJsonEncoded = body.GetAttributeValue("data-original-json", "");
            if (string.IsNullOrEmpty(originalJsonEncoded))
            {
                throw new Exception("Invalid HTML: Original JSON not found in body attribute");
            }

            var originalJson = HttpUtility.HtmlDecode(originalJsonEncoded);
            var originalComposition = JObject.Parse(originalJson);

            foreach (var prop in originalComposition.Properties())
            {
                compositionData[prop.Name] = prop.Value.DeepClone();
            }
        }

        // Now update with translated values using json-path
        var parameterDivs = doc.DocumentNode.SelectNodes("//div[@data-json-path]");
        if (parameterDivs != null)
        {
            foreach (var parameterDiv in parameterDivs)
            {
                var jsonPath = parameterDiv.GetAttributeValue("data-json-path", "");
                if (string.IsNullOrEmpty(jsonPath))
                    continue;

                var parameterObject = ResolveParameterObject(compositionData, jsonPath);
                if (parameterObject == null)
                    continue; // Path not found, skip

                // The composition itself is the source of truth for the parameter type. The attribute
                // is only a fallback, files produced by older versions can carry an incorrect type.
                var parameterType = parameterObject["type"]?.ToString();
                if (string.IsNullOrEmpty(parameterType))
                    parameterType = parameterDiv.GetAttributeValue("data-parameter-type", "");

                JToken translatedValue;
                if (string.Equals(parameterType, "richText", StringComparison.OrdinalIgnoreCase))
                {
                    translatedValue = new HtmlToRichTextConverter().ToRichText(parameterDiv.InnerHtml);
                }
                else
                {
                    var textNode = parameterDiv.SelectSingleNode(".//h1 | .//h2 | .//h3 | .//p");
                    if (textNode == null)
                        continue;

                    translatedValue = HttpUtility.HtmlDecode(textNode.InnerText);
                }

                SetLocaleValue(parameterObject, targetLocale, translatedValue);
            }
        }

        // Ensure target locale is in _locales array
        var locales = compositionData["_locales"] as JArray;
        if (locales == null)
        {
            locales = new JArray();
            compositionData["_locales"] = locales;
        }

        if (!locales.Any(l => l.ToString() == targetLocale))
        {
            locales.Add(targetLocale);
        }
    }

    /// <summary>
    /// Resolves the parameter object a json-path points at, e.g. "slots.content[0].parameters.title.locales.en-US"
    /// resolves to the 'title' parameter object (the last two segments address the locale value itself).
    /// </summary>
    private static JObject? ResolveParameterObject(JObject compositionData, string jsonPath)
    {
        var pathParts = ParseJsonPath(jsonPath);

        JToken? current = compositionData;

        // Navigate to the parent of the locale (stop before the last two parts: "locales" and locale code)
        for (int i = 0; i < pathParts.Count - 2; i++)
        {
            var part = pathParts[i];

            if (part.StartsWith("[") && part.EndsWith("]"))
            {
                // Array index
                var index = int.Parse(part.Substring(1, part.Length - 2));
                current = current?[index];
            }
            else
            {
                // Object property
                current = current?[part];
            }

            if (current == null)
            {
                return null; // Path not found
            }
        }

        return current as JObject;
    }

    private static void SetLocaleValue(JObject parameterObject, string targetLocale, JToken value)
    {
        var localesObj = parameterObject["locales"] as JObject;
        if (localesObj == null)
        {
            // Create locales object if it doesn't exist
            localesObj = new JObject();
            parameterObject["locales"] = localesObj;
        }

        localesObj[targetLocale] = value;
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
