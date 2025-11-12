using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Uniform.Models.Responses.Entries;

public class SearchEntriesResponse : ISearchContentOutput<EntryResponse>
{
    [Display("Entries")]
    public IEnumerable<EntryResponse> Items { get; set; } = new List<EntryResponse>();
    
    [Display("Total count")]
    public int TotalCount { get; set; }
}