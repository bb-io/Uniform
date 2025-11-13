using Apps.Uniform.Models.Responses.Entries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Uniform.Models.Dtos.Entries;

public class EntryWithFieldsDto : EntryResponse
{
    [JsonProperty("fields")]
    public JObject Fields { get; set; } = new();
}