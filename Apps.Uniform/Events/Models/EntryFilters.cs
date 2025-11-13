using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Uniform.Events.Models;

public class EntryFilters
{
    [Display("State"), StaticDataSource(typeof(StateDataHandler))]
    public string? State { get; set; }
}