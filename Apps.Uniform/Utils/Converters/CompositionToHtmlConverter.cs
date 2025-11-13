using Apps.Uniform.Models.Dtos.Canvas;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class CompositionToHtmlConverter
{
    private readonly List<ParameterDefinitionDto> _localizableParameters;
    private readonly string _locale;
    
    public CompositionToHtmlConverter(List<ParameterDefinitionDto> localizableParameters, string locale)
    {
        _localizableParameters = localizableParameters;
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
        
        var compositionDiv = doc.CreateElement("div");
        compositionDiv.SetAttributeValue("data-composition-id", compositionId);
        body.AppendChild(compositionDiv);
        
        // Process parameters
        var parameters = compositionData["parameters"] as JObject;
        if (parameters != null)
        {
            ProcessParameters(doc, compositionDiv, parameters);
        }
        
        // Process slots recursively
        var slots = compositionData["slots"] as JObject;
        if (slots != null)
        {
            ProcessSlots(doc, compositionDiv, slots);
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
    
    private void ProcessParameters(HtmlDocument doc, HtmlNode parentNode, JObject parameters)
    {
        foreach (var param in parameters)
        {
            var parameterId = param.Key;
            var parameterData = param.Value as JObject;
            
            if (parameterData == null) continue;
            
            var parameterDef = _localizableParameters.FirstOrDefault(p => p.Id == parameterId);
            if (parameterDef == null || !parameterDef.Localizable) continue;
            
            var parameterNode = ConvertParameterToHtml(doc, parameterDef, parameterData);
            if (parameterNode != null)
            {
                parentNode.AppendChild(parameterNode);
            }
        }
    }
    
    private void ProcessSlots(HtmlDocument doc, HtmlNode parentNode, JObject slots)
    {
        foreach (var slot in slots)
        {
            var slotArray = slot.Value as JArray;
            if (slotArray == null) continue;
            
            foreach (var component in slotArray)
            {
                var componentObj = component as JObject;
                if (componentObj == null) continue;
                
                // Process component parameters
                var componentParameters = componentObj["parameters"] as JObject;
                if (componentParameters != null)
                {
                    ProcessParameters(doc, parentNode, componentParameters);
                }
                
                // Recursively process nested slots
                var componentSlots = componentObj["slots"] as JObject;
                if (componentSlots != null)
                {
                    ProcessSlots(doc, parentNode, componentSlots);
                }
            }
        }
    }
    
    private HtmlNode? ConvertParameterToHtml(HtmlDocument doc, ParameterDefinitionDto parameter, JObject parameterData)
    {
        // Check if parameter has locales
        var locales = parameterData["locales"] as JObject;
        if (locales != null && locales.TryGetValue(_locale, out var localeValue))
        {
            return CreateParameterDiv(doc, parameter, localeValue);
        }
        
        // Check if parameter has direct value (non-localized format)
        var value = parameterData["value"];
        if (value != null && value.Type == JTokenType.String)
        {
            return CreateParameterDiv(doc, parameter, value);
        }
        
        return null;
    }
    
    private HtmlNode CreateParameterDiv(HtmlDocument doc, ParameterDefinitionDto parameter, JToken value)
    {
        var div = doc.CreateElement("div");
        div.SetAttributeValue("data-parameter-id", parameter.Id);
        div.SetAttributeValue("data-parameter-type", parameter.Type);
        
        var textValue = value.ToString();
        
        // Use heading tags for common title/heading parameters
        if (parameter.Id.Contains("title", StringComparison.OrdinalIgnoreCase) ||
            parameter.Name.Contains("title", StringComparison.OrdinalIgnoreCase) ||
            parameter.Id.Contains("heading", StringComparison.OrdinalIgnoreCase))
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
}
