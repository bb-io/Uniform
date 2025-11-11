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

namespace Apps.Uniform.Actions;

[ActionList("Entries")]
public class EntryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : Invocable(invocationContext)
{
    [Action("Search entries", Description = "Retrieve a list of entries based on specified criteria")]
    public async Task<SearchEntriesResponse> SearchEntries([ActionParameter] SearchEntryRequest searchEntryRequest)
    {
        var apiRequest = new RestRequest("/api/v1/entries");
        if (searchEntryRequest.State != null)
        {
            apiRequest.AddParameter("state", searchEntryRequest.State);
        }
        
        var dtoResponse = await Client.ExecuteWithErrorHandling<EntriesDto<EntryDto>>(apiRequest);
        return new SearchEntriesResponse()
        {
            Entries = dtoResponse.Entries.Select(e => e.Data).ToList(),
            TotalCount = dtoResponse.Entries.Count
        };
    }
    
    [Action("Download entry", Description = "Download entry as HTML file for translation")]
    public async Task<DownloadEntryResponse> DownloadEntry([ActionParameter] DownloadEntryRequest request)
    {
        // Get entry by ID
        var entryRequest = new RestRequest("/api/v1/entries");
        entryRequest.AddQueryParameter("entryIDs", request.EntryId);
        if (request.State != null)
        {
            entryRequest.AddQueryParameter("state", request.State);
        }
        
        var entriesResponse = await Client.ExecuteWithErrorHandling<EntriesDto<EntryDetailsDto>>(entryRequest);
        var entryDto = entriesResponse.Entries.FirstOrDefault();
        
        if (entryDto == null)
        {
            throw new Exception($"Entry with ID {request.EntryId} not found");
        }
        
        var entry = entryDto.Data;
        
        // Get content types to determine which fields are localizable
        var contentTypesRequest = new RestRequest("/api/v1/content-types");
        var contentTypesResponse = await Client.ExecuteWithErrorHandling<ContentTypesDto>(contentTypesRequest);
        
        var contentType = contentTypesResponse.ContentTypes.FirstOrDefault(ct => ct.Id == entry.ContentType);
        if (contentType == null)
        {
            throw new Exception($"Content type {entry.ContentType} not found");
        }
        
        var localizableFields = contentType.Fields.Where(f => f.Localizable).ToList();
        
        // Convert entry to JObject for processing
        var entryJson = JsonConvert.SerializeObject(entry);
        var entryData = JObject.Parse(entryJson);
        
        // Convert to HTML
        var converter = new EntryToHtmlConverter(localizableFields, request.Locale);
        var html = converter.ToHtml(entryData, entry.Id, entry.Name);
        
        // Upload HTML file
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
    public async Task UploadEntry([ActionParameter] UploadEntryRequest request)
    {
        // Download HTML file
        var fileStream = await fileManagementClient.DownloadAsync(request.Content);
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        var fileBytes = await memoryStream.GetByteData();
        var html = Encoding.UTF8.GetString(fileBytes);
        
        // Extract entry metadata from HTML
        var (entryId, originalLocale) = HtmlToEntryConverter.ExtractMetadata(html);
        
        if (string.IsNullOrEmpty(entryId))
        {
            throw new Exception("Invalid HTML: Entry ID not found in metadata");
        }
        
        // Get original entry
        var entryRequest = new RestRequest("/api/v1/entries");
        entryRequest.AddQueryParameter("entryIDs", entryId);
        if (request.State != null)
        {
            entryRequest.AddQueryParameter("state", request.State);
        }
        
        var entriesResponse = await Client.ExecuteWithErrorHandling<EntriesDto<EntryDetailsDto>>(entryRequest);
        var entryDto = entriesResponse.Entries.FirstOrDefault();
        
        if (entryDto == null)
        {
            throw new Exception($"Entry with ID {entryId} not found");
        }
        
        // Get content type to determine localizable fields
        var contentTypesRequest = new RestRequest("/api/v1/content-types");
        var contentTypesResponse = await Client.ExecuteWithErrorHandling<ContentTypesDto>(contentTypesRequest);
        
        var contentType = contentTypesResponse.ContentTypes.FirstOrDefault(ct => ct.Id == entryDto.Data.ContentType);
        if (contentType == null)
        {
            throw new Exception($"Content type {entryDto.Data.ContentType} not found");
        }
        
        var localizableFields = contentType.Fields.Where(f => f.Localizable).ToList();
        
        // Convert entry DTO to full entry structure with metadata
        var fullEntryJson = JsonConvert.SerializeObject(entryDto);
        var fullEntry = JObject.Parse(fullEntryJson);
        
        var state = int.TryParse(request.State, out var parsedState) ? parsedState : 0;
        fullEntry.Add("state", state);
        
        // Update entry from HTML
        var entryData = fullEntry["entry"] as JObject;
        if (entryData == null)
        {
            throw new Exception("Invalid entry structure");
        }
        
        var htmlConverter = new HtmlToEntryConverter(localizableFields);
        htmlConverter.UpdateEntryFromHtml(html, entryData, request.Locale);
        
        // Update entry via API
        var updateRequest = new RestRequest("/api/v1/entries", Method.Put);
        updateRequest.AddJsonBody(fullEntry.ToString());
        
        await Client.ExecuteWithErrorHandling(updateRequest);
    }
}
