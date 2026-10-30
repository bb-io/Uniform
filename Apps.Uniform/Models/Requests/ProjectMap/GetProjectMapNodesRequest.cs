using Apps.Uniform.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Uniform.Models.Requests.ProjectMap;

public class GetProjectMapNodesRequest
{
    [Display("Composition ID"), DataSource(typeof(CompositionDataHandler))]
    public string CompositionId { get; set; } = null!;
}
