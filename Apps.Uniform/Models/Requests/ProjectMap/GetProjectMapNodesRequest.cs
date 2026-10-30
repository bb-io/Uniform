using Blackbird.Applications.Sdk.Common;

namespace Apps.Uniform.Models.Requests.ProjectMap;

public class GetProjectMapNodesRequest
{
    [Display("Project ID")]
    public string ProjectId { get; set; } = null!;

    [Display("Composition ID")]
    public string CompositionId { get; set; } = null!;
}
