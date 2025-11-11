using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos;

public class ContentTypesDto
{
    [JsonProperty("contentTypes")]
    public List<ContentTypeDto> ContentTypes { get; set; } = new();
}

public class ContentTypeDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("fields")]
    public List<ContentTypeFieldDto> Fields { get; set; } = new();
}

public class ContentTypeFieldDto
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
