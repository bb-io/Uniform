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
using Apps.Uniform.Models.Dtos.Canvas;
using Apps.Uniform.Utils.Converters;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json.Linq;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff2;

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
    
    [Action("Download composition", Description = "Download composition as HTML file for translation")]
    public async Task<DownloadCompositionResponse> DownloadComposition([ActionParameter] DownloadCompositionRequest request)
    {
        var compositionRequest = new RestRequest("/api/v1/canvas");
        compositionRequest.AddQueryParameter("compositionId", request.CompositionId);
        if (request.State != null)
        {
            compositionRequest.AddQueryParameter("state", request.State);
        }
        
        var compositionResponse = await Client.ExecuteWithErrorHandling(compositionRequest);
        var compositionDto = JsonConvert.DeserializeObject<CompositionDetailsDto>(compositionResponse.Content ?? string.Empty);
        
        if (compositionDto == null)
        {
            throw new PluginApplicationException($"Composition with ID {request.CompositionId} not found");
        }
        
        var composition = compositionDto.Data;
        
        // Get canvas definitions to determine which parameters are localizable
        var canvasDefinitionsRequest = new RestRequest("/api/v1/canvas-definitions");
        var canvasDefinitionsResponse = await Client.ExecuteWithErrorHandling<CanvasDefinitionsDto>(canvasDefinitionsRequest);
        
        // Collect all localizable parameters from all component definitions
        var localizableParameters = canvasDefinitionsResponse.ComponentDefinitions
            .SelectMany(cd => cd.Parameters.Where(p => p.Localizable))
            .ToList();
        
        var compositionJson = JsonConvert.SerializeObject(composition);
        var compositionData = JObject.Parse(compositionJson);
        
        var converter = new CompositionToHtmlConverter(localizableParameters, request.Locale);
        var html = converter.ToHtml(compositionData, composition.Id, composition.Name, compositionDto.State.ToString());
        
        var fileReference = await fileManagementClient.UploadAsync(
            new MemoryStream(Encoding.UTF8.GetBytes(html)),
            MediaTypeNames.Text.Html,
            $"{composition.Name}_{request.Locale}.html");
        
        return new DownloadCompositionResponse
        {
            Content = fileReference
        };
    }
    
    [Action("Upload composition", Description = "Update composition from translated HTML file")]
    public async Task UploadComposition([ActionParameter] UploadCompositionRequest request)
    {
        var fileStream = await fileManagementClient.DownloadAsync(request.Content);
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var fileBytes = await memoryStream.GetByteData();
        var html = Encoding.UTF8.GetString(fileBytes);

        if (Xliff2Serializer.IsXliff2(html))
        {
            html = Transformation.Parse(html, request.Content.Name).Target().Serialize();
            if (html == null)
                throw new PluginMisconfigurationException("XLIFF did not contain any files");
        }

        var (compositionId, originalLocale) = HtmlToCompositionConverter.ExtractMetadata(html);
        if (string.IsNullOrEmpty(compositionId))
            throw new PluginApplicationException("Invalid HTML: Composition ID not found in metadata");

        // Get the original composition to extract state
        var compositionRequest = new RestRequest("/api/v1/canvas");
        compositionRequest.AddQueryParameter("compositionId", compositionId);

        var originalStateString = request.State ?? "0";
        if (!int.TryParse(originalStateString, out var originalState))
            originalState = 0;

        compositionRequest.AddQueryParameter("state", originalState.ToString());

        var compositionResponse = await Client.ExecuteWithErrorHandling(compositionRequest);
        var compositionDto = JsonConvert.DeserializeObject<CompositionDetailsDto>(compositionResponse.Content ?? string.Empty);
        if (compositionDto == null)
            throw new PluginApplicationException($"Composition with ID {compositionId} not found");

        var canvasDefinitionsRequest = new RestRequest("/api/v1/canvas-definitions");
        var projectId = GetProjectIdFromCreds();
        canvasDefinitionsRequest.AddQueryParameter("projectId", projectId);

        var canvasDefinitionsResponse = await Client.ExecuteWithErrorHandling<CanvasDefinitionsDto>(canvasDefinitionsRequest);

        var localizableParameters = canvasDefinitionsResponse.ComponentDefinitions
            .SelectMany(cd => cd.Parameters.Where(p => p.Localizable))
            .ToList();

        var compositionData = new JObject();
        var htmlConverter = new HtmlToCompositionConverter(localizableParameters);
        htmlConverter.UpdateCompositionFromHtml(html, compositionData, request.Locale);

        if (compositionData["_id"] == null || string.IsNullOrWhiteSpace(compositionData["_id"]?.ToString()))
            compositionData["_id"] = compositionId;

        // Remove unnecessary fields that shouldn't be in update request
        compositionData.Remove("pattern");
        compositionData.Remove("patternType");
        compositionData.Remove("categoryId");
        compositionData.Remove("description");
        compositionData.Remove("previewImageUrl");
        compositionData.Remove("editionId");
        compositionData.Remove("editionName");

        var fullComposition = new JObject
        {
            ["projectId"] = projectId,
            ["state"] = originalState,
            ["composition"] = compositionData
        };

        var updateRequest = new RestRequest("/api/v1/canvas", Method.Put);

        updateRequest.AddStringBody(fullComposition.ToString(Formatting.None), DataFormat.Json);

        await Client.ExecuteWithErrorHandling(updateRequest);
    }

    private string GetProjectIdFromCreds()
    {
        var projectId = InvocationContext.AuthenticationCredentialsProviders
            .FirstOrDefault(x => x.KeyName == "project_id")
            ?.Value;

        if (string.IsNullOrWhiteSpace(projectId))
            throw new PluginMisconfigurationException("Missing 'project_id' in connection credentials.");

        return projectId;
    }
}