using Apps.Uniform.Handlers;
using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Uniform.Models.Requests.Compositions;

public class DownloadCompositionRequest
{
    [Display("Composition ID"), DataSource(typeof(CompositionDataHandler))]
    public string CompositionId { get; set; } = string.Empty;
    
    [Display("Locale", Description = "The locale to download the composition for"), DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;
    
    [Display("State", Description = "Composition state (0=Draft, 64=Published)"), StaticDataSource(typeof(StateDataHandler))]
    public string? State { get; set; }
}
