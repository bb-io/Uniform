using Apps.Uniform.Models.Dtos.Entries;
using Apps.Uniform.Models.Requests.Entries;
using Apps.Uniform.Models.Responses.Entries;
using Apps.Uniform.Models.Dtos;
using Apps.Uniform.Utils.Converters;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using Apps.Uniform.Constants;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff2;

namespace Apps.Uniform.Actions;

[ActionList("Entries")]
public class EntryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : Invocable(invocationContext)
{
    [Action("Search entries", Description = "Retrieve a list of entries based on specified criteria")]
    [BlueprintActionDefinition(BlueprintAction.SearchContent)]
    public async Task<SearchEntriesResponse> SearchEntries([ActionParameter] SearchEntryRequest searchEntryRequest)
    {
        var apiRequest = new RestRequest("/api/v1/entries");
        if (searchEntryRequest.State != null)
        {
            apiRequest.AddParameter("state", searchEntryRequest.State);
        }
        
        if (searchEntryRequest.Locale != null)
        {
            apiRequest.AddParameter("locale", searchEntryRequest.Locale);
        }
        
        if (searchEntryRequest.Slug != null)
        {
            apiRequest.AddParameter("slug", searchEntryRequest.Slug);
        }
        
        var dtoResponse = await Client.AutoPaginateAsync<EntryDto>(apiRequest, json =>
            JsonConvert.DeserializeObject<EntriesDto<EntryDto>>(json)?.Entries ?? []);
        return new SearchEntriesResponse()
        {
            Items = dtoResponse.Select(e => e.Data).ToList(),
            TotalCount = dtoResponse.Count
        };
    }
    
    [Action("Get entry", Description = "Retrieve details of a specific entry by its ID")]
    public async Task<EntryResponse> GetEntry([ActionParameter] GetEntryRequest getEntryRequest)
    {
        var apiRequest = new RestRequest("/api/v1/entries");
        apiRequest.AddQueryParameter("entryIDs", getEntryRequest.ContentId);
        if (getEntryRequest.State != null)
        {
            apiRequest.AddQueryParameter("state", getEntryRequest.State);
        }
        
        var dtoResponse = await Client.ExecuteWithErrorHandling<EntriesDto<EntryDto>>(apiRequest);
        var entryDto = dtoResponse.Entries.FirstOrDefault();
        
        if (entryDto == null)
        {
            throw new Exception($"Entry with ID {getEntryRequest.ContentId} not found");
        }
        
        return entryDto.Data;
    }
    
    [Action("Download entry", Description = "Download entry as HTML file for translation")]
    [BlueprintActionDefinition(BlueprintAction.DownloadContent)]
    public async Task<DownloadEntryResponse> DownloadEntry([ActionParameter] DownloadEntryRequest request)
    {
        var entryRequest = new RestRequest("/api/v1/entries");
        entryRequest.AddQueryParameter("entryIDs", request.ContentId);
        if (request.State != null)
        {
            entryRequest.AddQueryParameter("state", request.State);
        }
        
        var entriesResponse = await Client.ExecuteWithErrorHandling<EntriesDto<EntryDetailsDto>>(entryRequest);
        var entryDto = entriesResponse.Entries.FirstOrDefault();
        
        if (entryDto == null)
        {
            throw new Exception($"Entry with ID {request.ContentId} not found");
        }
        
        var entry = entryDto.Data;
        
        var contentTypesRequest = new RestRequest("/api/v1/content-types");
        var contentTypesResponse = await Client.ExecuteWithErrorHandling<ContentTypesDto>(contentTypesRequest);
        
        var contentType = contentTypesResponse.ContentTypes.FirstOrDefault(ct => ct.Id == entry.ContentType);
        if (contentType == null)
        {
            throw new Exception($"Content type {entry.ContentType} not found");
        }
        
        var localizableFields = contentType.Fields.Where(f => f.Localizable).ToList();
        
        var entryJson = JsonConvert.SerializeObject(entry);
        var entryData = JObject.Parse(entryJson);
        
        var converter = new EntryToHtmlConverter(localizableFields, request.Locale);
        var html = converter.ToHtml(entryData, entry.Id, entry.Name, entryDto.State.ToString());
        
        var fileReference = await fileManagementClient.UploadAsync(
            new MemoryStream(Encoding.UTF8.GetBytes(html)),
            MediaTypeNames.Text.Html,
            $"{entry.Name}_{request.Locale}.html");
        
        return new DownloadEntryResponse
        {
            Content = fileReference
        };
    }
    
    [Action("Upload entry", Description = "Update entry from translated HTML file")]
    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    public async Task UploadEntry([ActionParameter] UploadEntryRequest request)
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
            {
                throw new PluginMisconfigurationException("XLIFF did not contain any files");
            }
        }
        
        var (entryId, originalLocale, state) = HtmlToEntryConverter.ExtractMetadata(html);
        if (string.IsNullOrEmpty(entryId))
        {
            throw new Exception("Invalid HTML: Entry ID not found in metadata");
        }
        
        var entryRequest = new RestRequest("/api/v1/entries");
        entryRequest.AddQueryParameter("entryIDs", entryId);
        if (!string.IsNullOrEmpty(state))
        {
            entryRequest.AddQueryParameter("state", state);
        }
        
        var entriesResponse = await Client.ExecuteWithErrorHandling<EntriesDto<EntryDetailsDto>>(entryRequest);
        var entryDto = entriesResponse.Entries.FirstOrDefault();
        
        if (entryDto == null)
        {
            throw new Exception($"Entry with ID {entryId} not found");
        }
        
        var contentTypesRequest = new RestRequest("/api/v1/content-types");
        var contentTypesResponse = await Client.ExecuteWithErrorHandling<ContentTypesDto>(contentTypesRequest);
        
        var contentType = contentTypesResponse.ContentTypes.FirstOrDefault(ct => ct.Id == entryDto.Data.ContentType);
        if (contentType == null)
        {
            throw new Exception($"Content type {entryDto.Data.ContentType} not found");
        }
        
        var localizableFields = contentType.Fields.Where(f => f.Localizable).ToList();
        
        var fullEntryJson = JsonConvert.SerializeObject(entryDto);
        var fullEntry = JObject.Parse(fullEntryJson);
        fullEntry.Add("state", state);
        
        var entryData = fullEntry["entry"] as JObject;
        if (entryData == null)
        {
            throw new Exception("Invalid entry structure");
        }
        
        var htmlConverter = new HtmlToEntryConverter(localizableFields);
        htmlConverter.UpdateEntryFromHtml(html, entryData, request.Locale);
        
        var updateRequest = new RestRequest("/api/v1/entries", Method.Put);
        updateRequest.AddJsonBody(fullEntry.ToString());
        
        await Client.ExecuteWithErrorHandling(updateRequest);
    }
    
    [Action("Delete entry", Description = "Deletes or unpublishes an entry by its ID")]
    public async Task DeleteEntry([ActionParameter] DeleteEntryRequest deleteEntryRequest)
    {
        var projectId = CredentialsProviders.Get(CredNames.ProjectId).Value;
        var bodyDictionary = new Dictionary<string, object>
        {
            { "entryId", deleteEntryRequest.ContentId },
            { "projectId", projectId }
        };
        
        if(deleteEntryRequest.State != null)
        {
            bodyDictionary.Add("state", deleteEntryRequest.State);
        }
        
        var apiRequest = new RestRequest($"/api/v1/entries/{deleteEntryRequest.ContentId}", Method.Delete)
            .AddJsonBody(bodyDictionary);
        
        await Client.ExecuteWithErrorHandling(apiRequest);
    }
}