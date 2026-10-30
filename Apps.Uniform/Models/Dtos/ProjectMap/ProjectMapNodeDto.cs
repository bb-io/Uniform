using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.ProjectMap;

public class ProjectMapNodeDto
{
    [JsonProperty("path")]
    public string? Path { get; set; }
}
