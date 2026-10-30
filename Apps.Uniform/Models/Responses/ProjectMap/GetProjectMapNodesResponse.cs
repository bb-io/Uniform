using Blackbird.Applications.Sdk.Common;

namespace Apps.Uniform.Models.Responses.ProjectMap;

public class GetProjectMapNodesResponse
{
    [Display("Root path")]
    public string RootPath { get; set; } = string.Empty;
}
