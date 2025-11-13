using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Uniform.Models.Responses.Compositions;

public class DownloadCompositionResponse
{
    public FileReference Content { get; set; } = null!;
}
