using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.Canvas;

public class CanvasDefinitionsDto
{
    [JsonProperty("componentDefinitions")]
    public List<ComponentDefinitionDto> ComponentDefinitions { get; set; } = new();
}

public class ComponentDefinitionDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("parameters")]
    public List<ParameterDefinitionDto> Parameters { get; set; } = new();
}

public class ParameterDefinitionDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonProperty("localizable")]
    public bool Localizable { get; set; }
}
