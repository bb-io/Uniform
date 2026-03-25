using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Responses.Entries;

public class PollingEntryResponse(EntryResponse entryResponse) : BaseEntryResponse, IDownloadContentInput
{
    [Display("Entry ID"), JsonProperty("_id")]
    public string ContentId { get; set; } = entryResponse.Id;
}
