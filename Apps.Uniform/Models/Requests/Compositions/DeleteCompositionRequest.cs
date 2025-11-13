using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Uniform.Models.Requests.Compositions;

public class DeleteCompositionRequest
{
    [Display("Composition ID")]
    public string CompositionId { get; set; } = null!;
    
    [Display("State"), StaticDataSource(typeof(StateDataHandler))]
    public string? State { get; set; }
}
