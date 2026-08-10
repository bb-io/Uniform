using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class CompositionToHtmlConverter
{
    // Parameter ids are not unique across component definitions (the same 'title' can be
    // 'text' in one component and 'richText' in another), so the type is always taken from
    // the parameter instance inside the composition itself.
    private static readonly HashSet<string> TranslatableParameterTypes =
        new(StringComparer.OrdinalIgnoreCase) { "text", "richText" };

    private readonly string _locale;

    public CompositionToHtmlConverter(string locale)
    {
        _locale = locale;
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

        // Process parameters
        var parameters = compositionData["parameters"] as JObject;
        if (parameters != null)
        {
            ProcessParameters(doc, compositionDiv, parameters, "parameters");
        }

        // Process slots recursively
        var slots = compositionData["slots"] as JObject;
        if (slots != null)
        {
            ProcessSlots(doc, compositionDiv, slots, "slots");
        }

        return doc.DocumentNode.OuterHtml;
    }

    private void AddMetaTag(HtmlDocument doc, HtmlNode head, string name, string content)
    {
        var meta = doc.CreateElement("meta");
        meta.SetAttributeValue("name", name);
        meta.SetAttributeValue("content", content);
        head.AppendChild(meta);
    }

    private void ProcessParameters(HtmlDocument doc, HtmlNode parentNode, JObject parameters, string basePath)
    {
        foreach (var param in parameters)
        {
            var parameterId = param.Key;

            if (param.Value is not JObject parameterData) continue;

            var parameterType = parameterData["type"]?.ToString();
            if (string.IsNullOrEmpty(parameterType) || !TranslatableParameterTypes.Contains(parameterType)) continue;

            // Only localized values are translatable, the locale has to be present on the parameter
            if (parameterData["locales"] is not JObject locales || !locales.TryGetValue(_locale, out var localeValue)) continue;

            var jsonPath = $"{basePath}.{parameterId}.locales.{_locale}";
            parentNode.AppendChild(CreateParameterDiv(doc, parameterId, parameterType, localeValue, jsonPath));
        }
    }

    private void ProcessSlots(HtmlDocument doc, HtmlNode parentNode, JObject slots, string basePath)
    {
        foreach (var slot in slots)
        {
            var slotArray = slot.Value as JArray;
            if (slotArray == null) continue;

            for (int i = 0; i < slotArray.Count; i++)
            {
                var componentObj = slotArray[i] as JObject;
                if (componentObj == null) continue;

                var slotPath = $"{basePath}.{slot.Key}[{i}]";

                // Process component parameters
                var componentParameters = componentObj["parameters"] as JObject;
                if (componentParameters != null)
                {
                    ProcessParameters(doc, parentNode, componentParameters, $"{slotPath}.parameters");
                }

                // Recursively process nested slots
                var componentSlots = componentObj["slots"] as JObject;
                if (componentSlots != null)
                {
                    ProcessSlots(doc, parentNode, componentSlots, $"{slotPath}.slots");
                }
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
