using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Uniform.Models.Requests.Compositions;

public class SearchCompositionRequest
{
    [Display("State"), StaticDataSource(typeof(StateDataHandler))]
    public string? State { get; set; }
    
    [Display("Keyword")]
    public string? Keyword { get; set; }
    
    [Display("Type")]
    public string? Type { get; set; }
    
    [Display("Slug")]
    public string? Slug { get; set; }
    
    [Display("Pattern")]
    public bool? Pattern { get; set; }
    
    [Display("Category ID")]
    public string? CategoryId { get; set; }
}
