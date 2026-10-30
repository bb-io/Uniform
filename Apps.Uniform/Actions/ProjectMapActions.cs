using Apps.Uniform.Models.Dtos.ProjectMap;
using Apps.Uniform.Models.Requests.ProjectMap;
using Apps.Uniform.Models.Responses.ProjectMap;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Uniform.Actions;

[ActionList("Project map")]
public class ProjectMapActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Get project map nodes", Description = "Get the project map path for a composition")]
    public async Task<GetProjectMapNodesResponse> GetProjectMapNodes(
        [ActionParameter] GetProjectMapNodesRequest request)
    {
        var apiRequest = new RestRequest("/api/v1/project-map-nodes")
            .AddQueryParameter("projectId", request.ProjectId)
            .AddQueryParameter("compositionId", request.CompositionId);

        var response = await Client.ExecuteWithErrorHandling(apiRequest);
        var projectMapNodes = JsonConvert.DeserializeObject<ProjectMapNodesDto>(response.Content ?? string.Empty);
        var rootPath = projectMapNodes?.Nodes.FirstOrDefault()?.Path;

        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new PluginApplicationException(
                $"No project map node found for composition with ID {request.CompositionId}");
        }

        return new GetProjectMapNodesResponse
        {
            RootPath = rootPath
        };
    }
}
