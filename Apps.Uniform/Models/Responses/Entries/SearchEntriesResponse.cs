using Blackbird.Applications.Sdk.Common;

namespace Apps.Uniform.Models.Responses.Entries;

public class SearchEntriesResponse
{
    [Display("Entries")]
    public List<EntryResponse> Entries { get; set; } = new();
    
    [Display("Total count")]
    public int TotalCount { get; set; }
}