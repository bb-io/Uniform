using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos;

public abstract class ProjectWrapperDto<T>
{
    [JsonProperty("projectId")]
    public string ProjectId { get; set; } = string.Empty;

    [JsonProperty("state")]
    public int State { get; set; }

    [JsonProperty("created")]
    public DateTime CreatedAt { get; set; }
    
    [JsonProperty("modified")]
    public DateTime? UpdatedAt { get; set; }

    [JsonProperty("creator")]
    public string? Creator { get; set; }

    [JsonProperty("author")]
    public string? Author { get; set; }
    
    public abstract T Data { get; set; }
}