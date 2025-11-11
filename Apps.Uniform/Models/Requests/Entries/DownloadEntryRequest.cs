using Blackbird.Applications.Sdk.Common;

namespace Apps.Uniform.Models.Requests.Entries;

public class DownloadEntryRequest
{
    [Display("Entry ID")]
    public string EntryId { get; set; } = string.Empty;
    
    [Display("Locale", Description = "The locale to download the entry for")]
    public string Locale { get; set; } = string.Empty;
    
    [Display("State", Description = "Entry state (0=Draft, 1=Published, etc.)")]
    public string? State { get; set; }
}
