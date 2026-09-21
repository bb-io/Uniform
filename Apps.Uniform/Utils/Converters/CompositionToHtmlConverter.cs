using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class CompositionToHtmlConverter
{
    // Parameter ids are not unique across component definitions (the same 'title' can be
    // 'text' in one component and 'richText' in another), so the type is always taken from
    // the parameter instance inside the composition itself.
    private static readonly HashSet<string> TranslatableParameterTypes =
        new(StringComparer.OrdinalIgnoreCase) { "text", "richText" };

    private static readonly Regex DynamicTokenExpression = new(@"\$\{.+?\}", RegexOptions.Compiled);

    private readonly string _locale;
    private readonly LocalizableParameterSet _localizableParameters;

    public CompositionToHtmlConverter(string locale, LocalizableParameterSet localizableParameters)
    {
        _locale = locale;
        _localizableParameters = localizableParameters;
    }

    public string ToHtml(JObject compositionData, string compositionId, string compositionName, string state)
    {
        var doc = new HtmlDocument();
        var html = doc.CreateElement("html");
        doc.DocumentNode.AppendChild(html);
        html.SetAttributeValue("lang", _locale);

        var head = doc.CreateElement("head");
        html.AppendChild(head);

        AddMetaTag(doc, head, "blackbird-composition-id", compositionId);
        AddMetaTag(doc, head, "blackbird-locale", _locale);
        AddMetaTag(doc, head, "blackbird-composition-name", compositionName);
        AddMetaTag(doc, head, "blackbird-composition-state", state);

        var body = doc.CreateElement("body");
        html.AppendChild(body);

        // Store original JSON in body attribute
        var originalJson = JsonConvert.SerializeObject(compositionData, Formatting.None);
        body.SetAttributeValue("data-original-json", HttpUtility.HtmlEncode(originalJson));

        var compositionDiv = doc.CreateElement("div");
        compositionDiv.SetAttributeValue("data-composition-id", compositionId);
        body.AppendChild(compositionDiv);

        ProcessComponent(doc, compositionDiv, compositionData, string.Empty);

        return doc.DocumentNode.OuterHtml;
    }

    private void AddMetaTag(HtmlDocument doc, HtmlNode head, string name, string content)
    {
        var meta = doc.CreateElement("meta");
        meta.SetAttributeValue("name", name);
        meta.SetAttributeValue("content", content);
        head.AppendChild(meta);
    }

    private void ProcessComponent(HtmlDocument doc, HtmlNode parentNode, JObject component, string basePath)
    {
        var componentType = component["type"]?.ToString();

        if (component["parameters"] is JObject parameters)
        {
            ProcessParameters(doc, parentNode, componentType, parameters, $"{basePath}parameters");
        }

        if (component["slots"] is JObject slots)
        {
            ProcessSlots(doc, parentNode, slots, $"{basePath}slots");
        }
    }

    private void ProcessParameters(HtmlDocument doc, HtmlNode parentNode, string? componentType, JObject parameters, string basePath)
    {
        foreach (var param in parameters)
        {
            var parameterId = param.Key;

            if (param.Value is not JObject parameterData) continue;

            var parameterType = parameterData["type"]?.ToString();
            if (string.IsNullOrEmpty(parameterType) || !TranslatableParameterTypes.Contains(parameterType)) continue;

            var sourceValue = ResolveSourceValue(parameterData, componentType, parameterId);
            if (sourceValue == null) continue;

            var jsonPath = $"{basePath}.{parameterId}.locales.{_locale}";
            var parameterDiv = CreateParameterDiv(doc, parameterId, parameterType, sourceValue, jsonPath);
            if (!HasTranslatableText(parameterDiv.InnerText)) continue;

            parentNode.AppendChild(parameterDiv);
        }
    }

    private JToken? ResolveSourceValue(JObject parameterData, string? componentType, string parameterId)
    {
        if (parameterData["locales"] is JObject locales)
        {
            return locales.TryGetValue(_locale, out var localeValue) ? localeValue : null;
        }

        return _localizableParameters.Contains(componentType, parameterId) ? parameterData["value"] : null;
    }

    private static bool HasTranslatableText(string text) =>
        !string.IsNullOrWhiteSpace(text) && !DynamicTokenExpression.IsMatch(text);

    private void ProcessSlots(HtmlDocument doc, HtmlNode parentNode, JObject slots, string basePath)
    {
        foreach (var slot in slots)
        {
            if (slot.Value is not JArray slotArray) continue;

            for (int i = 0; i < slotArray.Count; i++)
            {
                if (slotArray[i] is not JObject componentObj) continue;

                ProcessComponent(doc, parentNode, componentObj, $"{basePath}.{slot.Key}[{i}].");
            }
        }
    }

    private static HtmlNode CreateParameterDiv(HtmlDocument doc, string parameterId, string parameterType, JToken value, string jsonPath)
    {
        var div = doc.CreateElement("div");
        div.SetAttributeValue("data-parameter-id", parameterId);
        div.SetAttributeValue("data-parameter-type", parameterType);
        div.SetAttributeValue("data-json-path", jsonPath);

        if (string.Equals(parameterType, "richText", StringComparison.OrdinalIgnoreCase))
        {
            div.InnerHtml = value is JObject richText ? new RichTextToHtmlConverter(richText).ToHtml() : string.Empty;
            return div;
        }

        var textValue = value.Type == JTokenType.Null ? string.Empty : value.ToString();

        // Use heading tags for common title/heading parameters
        var tagName = parameterId.Contains("title", StringComparison.OrdinalIgnoreCase) ||
                      parameterId.Contains("heading", StringComparison.OrdinalIgnoreCase)
            ? "h2"
            : "p";

        var textNode = doc.CreateElement(tagName);
        textNode.InnerHtml = HttpUtility.HtmlEncode(textValue);
        div.AppendChild(textNode);

        return div;
    }
}
