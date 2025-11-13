using Blackbird.Applications.Sdk.Common;

namespace Apps.Uniform.Models.Responses.Compositions;

public class SearchCompositionsResponse
{
    [Display("Compositions")]
    public List<CompositionResponse> Items { get; set; } = new();
    
    [Display("Total count")]
    public int TotalCount { get; set; }
}
