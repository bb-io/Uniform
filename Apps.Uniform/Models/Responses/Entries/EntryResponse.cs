using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Responses.Entries;

public class EntryResponse : IContentOutput
{
    [Display("Entry ID"), JsonProperty("_id")]
    public string Id { get; set; } = string.Empty;
    
    [Display("Entry name"), JsonProperty("_name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Slug"), JsonProperty("_slug")]
    public string Slug { get; set; } = string.Empty;

    [Display("Content type"), JsonProperty("type")]
    public string ContentType { get; set; } = string.Empty;
    
    [Display("Supported languages"), JsonProperty("_locales")]
    public List<string> SupportedLocales { get; set; } = new();
}