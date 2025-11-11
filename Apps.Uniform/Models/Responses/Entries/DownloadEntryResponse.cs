using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Uniform.Models.Responses.Entries;

public class DownloadEntryResponse
{
    [Display("HTML file")]
    public FileReference Content { get; set; } = null!;
}
