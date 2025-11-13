using Apps.Uniform.Models.Dtos.Canvas;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class HtmlToCompositionConverter
{
    private readonly List<ParameterDefinitionDto> _localizableParameters;
    
    public HtmlToCompositionConverter(List<ParameterDefinitionDto> localizableParameters)
    {
        _localizableParameters = localizableParameters;
    }
    
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
        
        var compositionDiv = doc.DocumentNode.SelectSingleNode("//div[@data-composition-id]");
        if (compositionDiv == null)
        {
            throw new Exception("Invalid HTML: Composition div not found");
        }
        
        // Update parameters
        UpdateParametersFromHtml(doc, compositionData, targetLocale);
        
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
    
    private void UpdateParametersFromHtml(HtmlDocument doc, JObject compositionData, string targetLocale)
    {
        var parameterDivs = doc.DocumentNode.SelectNodes("//div[@data-parameter-id]");
        if (parameterDivs == null) return;
        
        // Track which parameters we've already updated by path
        var updatedPaths = new HashSet<string>();
        
        foreach (var parameterDiv in parameterDivs)
        {
            var parameterId = parameterDiv.GetAttributeValue("data-parameter-id", "");
            var parameterType = parameterDiv.GetAttributeValue("data-parameter-type", "");
            
            var parameterDef = _localizableParameters.FirstOrDefault(p => p.Id == parameterId);
            if (parameterDef == null) continue;
            
            UpdateParameterInComposition(compositionData, parameterId, parameterType, parameterDiv, targetLocale, updatedPaths);
        }
    }
    
    private void UpdateParameterInComposition(JObject compositionData, string parameterId, string parameterType, HtmlNode parameterDiv, string targetLocale, HashSet<string> updatedPaths)
    {
        // Find and update the next unprocessed occurrence of the parameter
        UpdateParameterRecursive(compositionData, parameterId, parameterType, parameterDiv, targetLocale, "", updatedPaths);
    }
    
    private bool UpdateParameterRecursive(JObject obj, string parameterId, string parameterType, HtmlNode parameterDiv, string targetLocale, string currentPath, HashSet<string> updatedPaths)
    {
        bool found = false;
        
        // Check parameters at current level
        var parameters = obj["parameters"] as JObject;
        if (parameters != null && parameters.TryGetValue(parameterId, out var parameterData))
        {
            var paramObj = parameterData as JObject;
            if (paramObj != null)
            {
                var paramPath = $"{currentPath}.parameters.{parameterId}";
                
                // Only update if we haven't processed this specific parameter yet
                if (!updatedPaths.Contains(paramPath))
                {
                    UpdateParameterValue(paramObj, parameterDiv, targetLocale);
                    updatedPaths.Add(paramPath);
                    return true; // Found and updated, stop searching
                }
            }
        }
        
        // Recursively check slots
        var slots = obj["slots"] as JObject;
        if (slots != null)
        {
            foreach (var slot in slots)
            {
                var slotArray = slot.Value as JArray;
                if (slotArray == null) continue;
                
                for (int i = 0; i < slotArray.Count; i++)
                {
                    var component = slotArray[i] as JObject;
                    if (component != null)
                    {
                        var slotPath = $"{currentPath}.slots.{slot.Key}[{i}]";
                        if (UpdateParameterRecursive(component, parameterId, parameterType, parameterDiv, targetLocale, slotPath, updatedPaths))
                        {
                            return true; // Found and updated, stop searching
                        }
                    }
                }
            }
        }
        
        return found;
    }
    
    private void UpdateParameterValue(JObject parameterObj, HtmlNode parameterDiv, string targetLocale)
    {
        var textNode = parameterDiv.SelectSingleNode(".//h1 | .//h2 | .//h3 | .//p");
        if (textNode == null) return;
        
        var text = HttpUtility.HtmlDecode(textNode.InnerText);
        
        // Check if parameter uses locales structure
        var locales = parameterObj["locales"] as JObject;
        if (locales != null)
        {
            // Always update the target locale value, even if it exists
            locales[targetLocale] = text;
        }
        else
        {
            // Create locales structure if it doesn't exist
            var currentValue = parameterObj["value"]?.ToString();
            locales = new JObject();
            
            // Preserve existing value if it exists
            if (!string.IsNullOrEmpty(currentValue))
            {
                // Try to determine the original locale from composition
                locales["en-US"] = currentValue; // Default fallback
            }
            
            locales[targetLocale] = text;
            parameterObj["locales"] = locales;
            
            // Remove the old 'value' property if locales is used
            // parameterObj.Remove("value");
        }
    }
}
