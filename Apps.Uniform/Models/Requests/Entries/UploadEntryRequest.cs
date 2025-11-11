using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Uniform.Models.Requests.Entries;

public class UploadEntryRequest
{
    [Display("HTML file")]
    public FileReference Content { get; set; } = null!;
    
    [Display("Target locale", Description = "The locale to update in the entry")]
    public string Locale { get; set; } = string.Empty;
    
    [Display("State", Description = "Entry state (0=Draft, 1=Published, etc.)")]
    public string? State { get; set; }
}
