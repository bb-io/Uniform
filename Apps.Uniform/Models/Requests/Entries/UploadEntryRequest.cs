using Apps.Uniform.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Uniform.Models.Requests.Entries;

public class UploadEntryRequest : IUploadContentInput
{
    public FileReference Content { get; set; } = null!;
    
    [Display("Target locale", Description = "The locale to update in the entry"), DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;

    [Display("Content ID", Description = "The ID of the content to update"), DataSource(typeof(EntryDataHandler))]
    public string? ContentId { get; set; }
}
