using Apps.Uniform.Handlers;
using Apps.Uniform.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Uniform.Models.Requests.Compositions;

public class UploadCompositionRequest
{
    public FileReference Content { get; set; } = null!;
    
    [Display("Target locale", Description = "The locale to update in the composition"), DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;

    [Display("Composition ID", Description = "The ID of the composition to update"), DataSource(typeof(CompositionDataHandler))]
    public string? CompositionId { get; set; }
    
    [Display("State", Description = "The state of the composition to update"), StaticDataSource(typeof(StateDataHandler))]
    public string? State { get; set; }
}
