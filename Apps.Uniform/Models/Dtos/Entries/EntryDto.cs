using Apps.Uniform.Models.Responses.Entries;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.Entries;

public class EntryDto : ProjectWrapperDto<EntryResponse>
{
    [JsonProperty("entry")]
    public override EntryResponse Data { get; set; } = new();
}