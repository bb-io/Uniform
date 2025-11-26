using Apps.Uniform.Models.Dtos.Canvas;
using HtmlAgilityPack;
using Newtonsoft.Json;
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
        
        var body = doc.DocumentNode.SelectSingleNode("//body");
        if (body == null)
        {
            throw new Exception("Invalid HTML: Body element not found");
        }
        
        // Extract original JSON from body attribute
        var originalJsonEncoded = body.GetAttributeValue("data-original-json", "");
        if (string.IsNullOrEmpty(originalJsonEncoded))
        {
            throw new Exception("Invalid HTML: Original JSON not found in body attribute");
        }
        
        var originalJson = HttpUtility.HtmlDecode(originalJsonEncoded);
        var originalComposition = JObject.Parse(originalJson);
        
        // Copy the original composition structure to compositionData
        foreach (var prop in originalComposition.Properties())
        {
            compositionData[prop.Name] = prop.Value.DeepClone();
        }
        
        // Now update with translated values using json-path
        var parameterDivs = doc.DocumentNode.SelectNodes("//div[@data-json-path]");
        if (parameterDivs != null)
        {
            foreach (var parameterDiv in parameterDivs)
            {
                var jsonPath = parameterDiv.GetAttributeValue("data-json-path", "");
                if (string.IsNullOrEmpty(jsonPath)) continue;
                
                var textNode = parameterDiv.SelectSingleNode(".//h1 | .//h2 | .//h3 | .//p");
                if (textNode == null) continue;
                
                var translatedText = HttpUtility.HtmlDecode(textNode.InnerText);
                
                // Update the value at the json-path
                UpdateValueByJsonPath(compositionData, jsonPath, translatedText, targetLocale);
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
    
    private void UpdateValueByJsonPath(JObject compositionData, string jsonPath, string value, string targetLocale)
    {
        // Parse the json-path (format: "parameters.text.locales.en-US" or "slots.component[0].parameters.text.locales.en-US")
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
        
        // Navigate to the target location
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
                return; // Path not found, skip
            }
        }
        
        // Now we should be at the parameter object, and need to update locales
        var localesObj = current["locales"] as JObject;
        if (localesObj == null)
        {
            // Create locales object if it doesn't exist
            localesObj = new JObject();
            current["locales"] = localesObj;
        }
        
        // Set the target locale value
        localesObj[targetLocale] = value;
    }
}
