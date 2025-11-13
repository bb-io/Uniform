using Apps.Uniform.Handlers;
using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Uniform.Models.Requests.Entries;

public class SearchEntryRequest
{
    [Display("State"), StaticDataSource(typeof(StateDataHandler))]
    public string? State { get; set; }
    
    [Display("Locale"), DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }
    
    [Display("Slug")]
    public string? Slug { get; set; }
}