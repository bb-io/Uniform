using Newtonsoft.Json;

namespace Apps.Uniform.Models.Dtos.ProjectMap;

public class ProjectMapNodesDto
{
    [JsonProperty("nodes")]
    public List<ProjectMapNodeDto> Nodes { get; set; } = [];
}
