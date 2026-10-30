using Blackbird.Applications.Sdk.Common;

namespace Apps.Uniform.Models.Responses.Compositions;

public class GetCompositionResponse : CompositionResponse
{
    [Display("Project ID")]
    public string ProjectId { get; set; } = string.Empty;
}
