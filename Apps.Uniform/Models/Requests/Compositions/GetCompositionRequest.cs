using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Apps.Uniform.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Uniform.Models.Requests.Compositions;

public class GetCompositionRequest
{
    [Display("Composition ID") , DataSource(typeof(CompositionDataHandler))]
    public string CompositionId { get; set; } = null!;
    
    [Display("State"), StaticDataSource(typeof(StateDataHandler))]
    public string? State { get; set; }
}
