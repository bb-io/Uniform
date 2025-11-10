using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos;

public abstract class ProjectWrapperDto<T>
{
    [JsonProperty("projectId")]
    public string ProjectId { get; set; } = string.Empty;
    
    public abstract T Data { get; set; }
}