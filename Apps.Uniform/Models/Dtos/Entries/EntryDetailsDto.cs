using Apps.Uniform.Models.Responses.Entries;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.Entries;

public class EntryDetailsDto : ProjectWrapperDto<EntryWithFieldsDto>
{
    [JsonProperty("entry")]
    public override EntryWithFieldsDto Data { get; set; } = new();
}
