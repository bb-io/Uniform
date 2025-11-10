using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Uniform.Models.Requests.Entries;

public class SearchEntryRequest
{
    [Display("State"), StaticDataSource(typeof(EntryStateDataHandler))]
    public string? State { get; set; }
}