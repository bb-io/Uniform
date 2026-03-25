using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Responses.Entries;

public class EntryResponse : BaseEntryResponse, IContentOutput
{
    [Display("Entry ID"), JsonProperty("_id")]
    public string Id { get; set; } = string.Empty;
}