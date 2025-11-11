using Apps.Uniform.Models.Dtos;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class HtmlToEntryConverter
{
    private readonly List<ContentTypeFieldDto> _localizableFields;
    
    public HtmlToEntryConverter(List<ContentTypeFieldDto> localizableFields)
    {
        _localizableFields = localizableFields;
    }
    
    public static (string entryId, string locale, string state) ExtractMetadata(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        var entryIdMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-entry-id']");
        var localeMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-locale']");
        var stateMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-entry-state']");
        
        var entryId = entryIdMeta?.GetAttributeValue("content", "") ?? "";
        var locale = localeMeta?.GetAttributeValue("content", "") ?? "";
        var state = stateMeta?.GetAttributeValue("content", "") ?? "";
        
        return (entryId, locale, state);
    }
    
    public void UpdateEntryFromHtml(string html, JObject entryData, string targetLocale)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        var entryDiv = doc.DocumentNode.SelectSingleNode("//div[@data-entry-id]");
        if (entryDiv == null)
        {
            throw new Exception("Invalid HTML: Entry div not found");
        }
        
        var fields = entryData["fields"] as JObject;
        if (fields == null)
        {
            fields = new JObject();
            entryData["fields"] = fields;
        }
        
        foreach (var fieldDiv in entryDiv.SelectNodes(".//div[@data-field-id]") ?? Enumerable.Empty<HtmlNode>())
        {
            var fieldId = fieldDiv.GetAttributeValue("data-field-id", "");
            var fieldType = fieldDiv.GetAttributeValue("data-field-type", "");
            
            var field = _localizableFields.FirstOrDefault(f => f.Id == fieldId);
            if (field == null)
                continue;
            
            UpdateFieldFromHtml(fields, field, fieldDiv, targetLocale);
        }
        
        // Ensure target locale is in _locales array
        var locales = entryData["_locales"] as JArray;
        if (locales == null)
        {
            locales = new JArray();
            entryData["_locales"] = locales;
        }
        
        if (!locales.Any(l => l.ToString() == targetLocale))
        {
            locales.Add(targetLocale);
        }
    }
    
    private void UpdateFieldFromHtml(JObject fields, ContentTypeFieldDto field, HtmlNode fieldDiv, string targetLocale)
    {
        if (!fields.TryGetValue(field.Id, out var fieldData))
        {
            fieldData = new JObject
            {
                ["type"] = field.Type,
                ["locales"] = new JObject()
            };
            fields[field.Id] = fieldData;
        }
        
        var locales = fieldData["locales"] as JObject;
        if (locales == null)
        {
            locales = new JObject();
            fieldData["locales"] = locales;
        }
        
        switch (field.Type)
        {
            case "text":
                UpdateTextField(locales, fieldDiv, targetLocale);
                break;
            case "richText":
                UpdateRichTextField(locales, fieldDiv, targetLocale);
                break;
            case "number":
                UpdateNumberField(locales, fieldDiv, targetLocale);
                break;
            case "select":
                UpdateSelectField(locales, fieldDiv, targetLocale);
                break;
            default:
                // For unknown types, try to update as text
                UpdateTextField(locales, fieldDiv, targetLocale);
                break;
        }
    }
    
    private void UpdateTextField(JObject locales, HtmlNode fieldDiv, string targetLocale)
    {
        var textNode = fieldDiv.SelectSingleNode(".//h1 | .//h2 | .//p");
        if (textNode != null)
        {
            var text = HttpUtility.HtmlDecode(textNode.InnerText);
            locales[targetLocale] = text;
        }
    }
    
    private void UpdateRichTextField(JObject locales, HtmlNode fieldDiv, string targetLocale)
    {
        var htmlContent = fieldDiv.InnerHtml;
        var htmlToRichTextConverter = new HtmlToRichTextConverter();
        var richText = htmlToRichTextConverter.ToRichText(htmlContent);
        locales[targetLocale] = richText;
    }
    
    private void UpdateNumberField(JObject locales, HtmlNode fieldDiv, string targetLocale)
    {
        var textNode = fieldDiv.SelectSingleNode(".//p");
        if (textNode != null)
        {
            var text = HttpUtility.HtmlDecode(textNode.InnerText);
            if (decimal.TryParse(text, out var number))
            {
                locales[targetLocale] = number;
            }
        }
    }
    
    private void UpdateSelectField(JObject locales, HtmlNode fieldDiv, string targetLocale)
    {
        var textNode = fieldDiv.SelectSingleNode(".//p");
        if (textNode != null)
        {
            var text = HttpUtility.HtmlDecode(textNode.InnerText);
            locales[targetLocale] = text;
        }
    }
}
