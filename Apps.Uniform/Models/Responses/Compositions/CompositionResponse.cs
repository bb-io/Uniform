using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Uniform.Models.Responses.Compositions;

public class CompositionResponse
{
    [Display("Composition ID"), JsonProperty("_id")]
    public string Id { get; set; } = string.Empty;
    
    [Display("Composition name"), JsonProperty("_name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Slug"), JsonProperty("_slug")]
    public string? Slug { get; set; }
    
    [Display("Type"), JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
    
    [Display("Pattern"), JsonProperty("pattern")]
    public bool? Pattern { get; set; }
    
    [Display("Pattern type"), JsonProperty("patternType")]
    public string? PatternType { get; set; }
    
    [Display("Category ID"), JsonProperty("categoryId")]
    public string? CategoryId { get; set; }
    
    [Display("Description"), JsonProperty("description")]
    public string? Description { get; set; }
    
    [Display("Preview image URL"), JsonProperty("previewImageUrl")]
    public string? PreviewImageUrl { get; set; }
    
    [Display("Edition ID"), JsonProperty("editionId")]
    public string? EditionId { get; set; }
    
    [Display("Edition name"), JsonProperty("editionName")]
    public string? EditionName { get; set; }
}
