using Apps.Uniform.Models.Dtos.Compositions;
using Apps.Uniform.Models.Requests.Compositions;
using Apps.Uniform.Models.Responses.Compositions;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Uniform.Actions;

[ActionList("Compositions")]
public class CompositionActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : Invocable(invocationContext)
{
    [Action("Search compositions", Description = "Retrieve a list of compositions based on specified criteria")]
    public async Task<SearchCompositionsResponse> SearchCompositions([ActionParameter] SearchCompositionRequest searchRequest)
    {
        var apiRequest = new RestRequest("/api/v1/canvas")
            .AddQueryParameter("withTotalCount", "true");
        
        if (searchRequest.State != null)
        {
            apiRequest.AddParameter("state", searchRequest.State);
        }
        
        if (searchRequest.Keyword != null)
        {
            apiRequest.AddParameter("keyword", searchRequest.Keyword);
        }
        
        if (searchRequest.Type != null)
        {
            apiRequest.AddParameter("type", searchRequest.Type);
        }
        
        if (searchRequest.Slug != null)
        {
            apiRequest.AddParameter("slug", searchRequest.Slug);
        }
        
        if (searchRequest.Pattern != null)
        {
            apiRequest.AddParameter("pattern", searchRequest.Pattern.Value);
        }
        
        if (searchRequest.CategoryId != null)
        {
            apiRequest.AddParameter("categories", searchRequest.CategoryId);
        }

        var response = await Client.ExecuteWithErrorHandling(apiRequest);
        var responseDto = JsonConvert.DeserializeObject<CompositionsDto<CompositionDto>>(response.Content ?? string.Empty);
        
        if (responseDto == null)
        {
            throw new PluginApplicationException("Failed to deserialize compositions response");
        }

        return new SearchCompositionsResponse
        {
            Items = responseDto.Compositions.Select(c => c.Data).ToList(),
            TotalCount = responseDto.TotalCount ?? responseDto.Compositions.Count
        };
    }
    
    [Action("Get composition", Description = "Retrieve details of a specific composition by its ID")]
    public async Task<CompositionResponse> GetComposition([ActionParameter] GetCompositionRequest getRequest)
    {
        var apiRequest = new RestRequest("/api/v1/canvas")
            .AddQueryParameter("compositionId", getRequest.CompositionId);
        
        if (getRequest.State != null)
        {
            apiRequest.AddParameter("state", getRequest.State);
        }

        var response = await Client.ExecuteWithErrorHandling(apiRequest);
        var compositionDto = JsonConvert.DeserializeObject<CompositionDto>(response.Content ?? string.Empty);
        
        if (compositionDto == null)
        {
            throw new PluginApplicationException($"Composition with ID {getRequest.CompositionId} not found");
        }

        return compositionDto.Data;
    }
    
    [Action("Delete composition", Description = "Deletes or unpublishes a composition by its ID")]
    public async Task DeleteComposition([ActionParameter] DeleteCompositionRequest deleteRequest)
    {
        var bodyDictionary = new Dictionary<string, object?>
        {
            { "compositionId", deleteRequest.CompositionId },
            { "projectId", InvocationContext.AuthenticationCredentialsProviders.Get(Constants.CredNames.ProjectId).Value }
        };
        
        if (deleteRequest.State != null)
        {
            bodyDictionary.Add("state", int.Parse(deleteRequest.State));
        }
        
        
        var apiRequest = new RestRequest("/api/v1/canvas", Method.Delete)
            .AddJsonBody(bodyDictionary);

        await Client.ExecuteWithErrorHandling(apiRequest);
    }
}