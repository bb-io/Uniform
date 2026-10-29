using Apps.Uniform.Handlers;
using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

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

    [Display("Created after")]
    public DateTime? CreatedAfter { get; set; }

    [Display("Created before")]
    public DateTime? CreatedBefore { get; set; }

    [Display("Updated after")]
    public DateTime? UpdatedAfter { get; set; }

    [Display("Updated before")]
    public DateTime? UpdatedBefore { get; set; }

    [Display("Include creators"), DataSource(typeof(CreatorDataHandler))]
    public IEnumerable<string>? IncludeCreators { get; set; }

    [Display("Exclude creators"), DataSource(typeof(CreatorDataHandler))]
    public IEnumerable<string>? ExcludeCreators { get; set; }

    [Display("Include authors"), DataSource(typeof(AuthorDataHandler))]
    public IEnumerable<string>? IncludeAuthors { get; set; }

    [Display("Exclude authors"), DataSource(typeof(AuthorDataHandler))]
    public IEnumerable<string>? ExcludeAuthors { get; set; }
}
