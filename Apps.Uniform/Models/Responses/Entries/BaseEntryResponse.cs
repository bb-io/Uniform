using Newtonsoft.Json;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Uniform.Models.Responses.Entries;

public class BaseEntryResponse
{
    [Display("Entry name"), JsonProperty("_name")]
    public string Name { get; set; } = string.Empty;

    [Display("Slug"), JsonProperty("_slug")]
    public string Slug { get; set; } = string.Empty;

    [Display("Content type"), JsonProperty("type")]
    public string ContentType { get; set; } = string.Empty;

    [Display("Supported languages"), JsonProperty("_locales")]
    public List<string> SupportedLocales { get; set; } = new();
}
