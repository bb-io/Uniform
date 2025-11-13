using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Uniform.Models.Responses.Entries;

public class DownloadEntryResponse : IDownloadContentOutput
{
    public FileReference Content { get; set; } = null!;
}
