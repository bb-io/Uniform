using Apps.Uniform.Models.Responses.Compositions;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.Compositions;

public class CompositionDto : ProjectWrapperDto<CompositionResponse>
{
    [JsonProperty("composition")]
    public override CompositionResponse Data { get; set; } = new();
}
