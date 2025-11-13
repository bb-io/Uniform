using Apps.Uniform.Models.Responses.Compositions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Uniform.Models.Dtos.Compositions;

public class CompositionWithDetailsDto : CompositionResponse
{
    [JsonProperty("parameters")]
    public JObject? Parameters { get; set; }
    
    [JsonProperty("slots")]
    public JObject? Slots { get; set; }
    
    [JsonProperty("_locales")]
    public List<string> Locales { get; set; } = new();
}
