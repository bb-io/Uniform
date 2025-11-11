using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.Locales;

public class LocaleDto
{
    [JsonProperty("locale")]
    public string Locale { get; set; } = null!;
    
    [JsonProperty("displayName")]
    public string Name { get; set; } = null!;
}