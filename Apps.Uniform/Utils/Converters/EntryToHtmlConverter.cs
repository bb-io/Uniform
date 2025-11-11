using Apps.Uniform.Models.Dtos;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class EntryToHtmlConverter
{
    private readonly List<ContentTypeFieldDto> _localizableFields;
    private readonly string _locale;
    
    public EntryToHtmlConverter(List<ContentTypeFieldDto> localizableFields, string locale)
    {
        _localizableFields = localizableFields;
        _locale = locale;
    }
    
    public string ToHtml(JObject entryData, string entryId, string entryName)
    {
        var doc = new HtmlDocument();
        var html = doc.CreateElement("html");
        doc.DocumentNode.AppendChild(html);
        html.SetAttributeValue("lang", _locale);
        
        var head = doc.CreateElement("head");
        html.AppendChild(head);
        
        AddMetaTag(doc, head, "blackbird-entry-id", entryId);
        AddMetaTag(doc, head, "blackbird-locale", _locale);
        AddMetaTag(doc, head, "blackbird-entry-name", entryName);
        
        var body = doc.CreateElement("body");
        html.AppendChild(body);
        
        var entryDiv = doc.CreateElement("div");
        entryDiv.SetAttributeValue("data-entry-id", entryId);
        body.AppendChild(entryDiv);
        
        var fields = entryData["fields"] as JObject;
        if (fields != null)
        {
            foreach (var field in _localizableFields)
            {
                if (fields.TryGetValue(field.Id, out var fieldData))
                {
                    var fieldNode = ConvertFieldToHtml(doc, field, fieldData);
                    if (fieldNode != null)
                    {
                        entryDiv.AppendChild(fieldNode);
                    }
                }
            }
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
    
    private HtmlNode? ConvertFieldToHtml(HtmlDocument doc, ContentTypeFieldDto field, JToken fieldData)
    {
        var locales = fieldData["locales"] as JObject;
        if (locales == null || !locales.TryGetValue(_locale, out var localeValue))
        {
            return null;
        }
        
        switch (field.Type)
        {
            case "text":
                return ConvertTextFieldToHtml(doc, field, localeValue);
            case "richText":
                return ConvertRichTextFieldToHtml(doc, field, localeValue);
            case "asset":
                return ConvertAssetFieldToHtml(doc, field, localeValue);
            case "number":
                return ConvertNumberFieldToHtml(doc, field, localeValue);
            case "select":
                return ConvertSelectFieldToHtml(doc, field, localeValue);
            case "contentReference":
                return ConvertContentReferenceToHtml(doc, field, localeValue);
            default:
                // For unknown types, try to convert as text
                return ConvertTextFieldToHtml(doc, field, localeValue);
        }
    }
    
    private HtmlNode ConvertTextFieldToHtml(HtmlDocument doc, ContentTypeFieldDto field, JToken value)
    {
        var div = doc.CreateElement("div");
        div.SetAttributeValue("data-field-id", field.Id);
        div.SetAttributeValue("data-field-type", field.Type);
        
        var textValue = value.ToString();
        
        // Use h1 for title fields, h2 for subtitle-like fields
        if (field.Id.Equals("title", StringComparison.OrdinalIgnoreCase) || 
            field.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
        {
            var h1 = doc.CreateElement("h1");
            h1.InnerHtml = HttpUtility.HtmlEncode(textValue);
            div.AppendChild(h1);
        }
        else if (field.Id.Contains("subtitle", StringComparison.OrdinalIgnoreCase) ||
                 field.Name.Contains("subtitle", StringComparison.OrdinalIgnoreCase))
        {
            var h2 = doc.CreateElement("h2");
            h2.InnerHtml = HttpUtility.HtmlEncode(textValue);
            div.AppendChild(h2);
        }
        else
        {
            var p = doc.CreateElement("p");
            p.InnerHtml = HttpUtility.HtmlEncode(textValue);
            div.AppendChild(p);
        }
        
        return div;
    }
    
    private HtmlNode ConvertRichTextFieldToHtml(HtmlDocument doc, ContentTypeFieldDto field, JToken value)
    {
        var div = doc.CreateElement("div");
        div.SetAttributeValue("data-field-id", field.Id);
        div.SetAttributeValue("data-field-type", field.Type);
        
        if (value.Type == JTokenType.Object)
        {
            var richTextConverter = new RichTextToHtmlConverter(value as JObject ?? new JObject());
            var html = richTextConverter.ToHtml();
            div.InnerHtml = html;
        }
        
        return div;
    }
    
    private HtmlNode? ConvertAssetFieldToHtml(HtmlDocument doc, ContentTypeFieldDto field, JToken value)
    {
        // Assets are arrays of asset objects
        if (value.Type != JTokenType.Array)
        {
            return null;
        }
        
        var assets = value as JArray;
        if (assets == null || assets.Count == 0)
        {
            return null;
        }
        
        var div = doc.CreateElement("div");
        div.SetAttributeValue("data-field-id", field.Id);
        div.SetAttributeValue("data-field-type", field.Type);
        
        foreach (var asset in assets)
        {
            var assetId = asset["_id"]?.ToString();
            var url = asset["fields"]?["url"]?["value"]?.ToString();
            var title = asset["fields"]?["title"]?["value"]?.ToString();
            
            if (!string.IsNullOrEmpty(url))
            {
                var img = doc.CreateElement("img");
                img.SetAttributeValue("src", url);
                img.SetAttributeValue("data-asset-id", assetId ?? "");
                if (!string.IsNullOrEmpty(title))
                {
                    img.SetAttributeValue("alt", title);
                }
                div.AppendChild(img);
            }
        }
        
        return div;
    }
    
    private HtmlNode ConvertNumberFieldToHtml(HtmlDocument doc, ContentTypeFieldDto field, JToken value)
    {
        var div = doc.CreateElement("div");
        div.SetAttributeValue("data-field-id", field.Id);
        div.SetAttributeValue("data-field-type", field.Type);
        
        var p = doc.CreateElement("p");
        p.InnerHtml = HttpUtility.HtmlEncode(value.ToString());
        div.AppendChild(p);
        
        return div;
    }
    
    private HtmlNode ConvertSelectFieldToHtml(HtmlDocument doc, ContentTypeFieldDto field, JToken value)
    {
        var div = doc.CreateElement("div");
        div.SetAttributeValue("data-field-id", field.Id);
        div.SetAttributeValue("data-field-type", field.Type);
        
        var p = doc.CreateElement("p");
        p.InnerHtml = HttpUtility.HtmlEncode(value.ToString());
        div.AppendChild(p);
        
        return div;
    }
    
    private HtmlNode? ConvertContentReferenceToHtml(HtmlDocument doc, ContentTypeFieldDto field, JToken value)
    {
        // Content references are not translatable in MVP version
        return null;
    }
}
