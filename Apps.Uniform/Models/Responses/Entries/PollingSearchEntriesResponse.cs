using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Uniform.Models.Responses.Entries;

public record PollingSearchEntriesResponse(List<PollingEntryResponse> Items, int TotalCount) 
    : IMultiDownloadableContentOutput<PollingEntryResponse>
{
    [Display("Items")]
    public List<PollingEntryResponse> Items { get; set; } = Items;

    [Display("Total count")]
    public int TotalCount { get; set; } = TotalCount;
}
