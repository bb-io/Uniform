using Apps.Uniform.Models.Responses.Compositions;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.Compositions;

public class CompositionDetailsDto : ProjectWrapperDto<CompositionWithDetailsDto>
{
    [JsonProperty("composition")]
    public override CompositionWithDetailsDto Data { get; set; } = new();
}
