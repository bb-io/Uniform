using Apps.Uniform.Handlers;
using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Uniform.Models.Requests.Entries;

public class DownloadEntryRequest : IDownloadContentInput
{
    [Display("Entry ID"), DataSource(typeof(EntryDataHandler))]
    public string ContentId { get; set; } = string.Empty;
    
    [Display("Locale", Description = "The locale to download the entry for"), DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;
    
    [Display("State", Description = "Entry state (0=Draft, 64=Published)"), StaticDataSource(typeof(EntryStateDataHandler))]
    public string? State { get; set; }
}