using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.Locales;

public class LocalesDto
{
    [JsonProperty("results")]
    public List<LocaleDto> Locales { get; set; } = null!;
}