using Apps.Uniform.Models.Responses.Compositions;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.Compositions;

public class GetCompositionDto : ProjectWrapperDto<GetCompositionResponse>
{
    [JsonProperty("composition")]
    public override GetCompositionResponse Data { get; set; } = new();
}
