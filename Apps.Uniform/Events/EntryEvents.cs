using Apps.Uniform.Events.Models;
using Apps.Uniform.Models.Dtos.Entries;
using Apps.Uniform.Models.Responses.Entries;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Uniform.Events;

[PollingEventList("Entries")]
public class EntryEvents(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const int PageLimit = 150;
    
    [PollingEvent("On entries created", Description = "Polling event that periodically checks for newly created entries")]
    public async Task<PollingEventResponse<DateMemory, SearchEntriesResponse>> OnEntriesCreatedAsync(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] EntryFilters filters)
    {
        return await ProcessPollingRequestAsync(
            request,
            filters.State ?? "0",
            "created_at_DESC",
            e => e.CreatedAt > request.Memory!.LastPollingTime);
    }

    [PollingEvent("On entries updated", Description = "Polling event that periodically checks for updated entries")]
    public async Task<PollingEventResponse<DateMemory, SearchEntriesResponse>> OnEntriesUpdatedAsync(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] EntryFilters filters)
    {
        return await ProcessPollingRequestAsync(
            request,
            filters.State ?? "0",
            "updated_at_DESC",
            e => e.UpdatedAt.HasValue && e.UpdatedAt.Value > request.Memory!.LastPollingTime);
    }

    [PollingEvent("On entries published", Description = "Polling event that periodically checks for newly published or updated published entries")]
    [BlueprintEventDefinition(BlueprintEvent.ContentCreatedOrUpdated)]
    public async Task<PollingEventResponse<DateMemory, SearchEntriesResponse>> OnEntriesPublishedAsync(
        PollingEventRequest<DateMemory> request)
    {
        if (request.Memory == null)
        {
            return CreateInitialResponse();
        }

        var allPublishedEntries = new List<EntryResponse>();

        // Check for newly created published entries
        var createdEntries = await FetchEntriesAsync("64", "created_at_DESC");
        var newPublishedEntries = createdEntries
            .Where(e => e.CreatedAt > request.Memory.LastPollingTime)
            .Select(e => e.Data)
            .ToList();
        allPublishedEntries.AddRange(newPublishedEntries);

        // Check for updated published entries
        var updatedEntries = await FetchEntriesAsync("64", "updated_at_DESC");
        var updatedPublishedEntries = updatedEntries
            .Where(e => e.UpdatedAt.HasValue && e.UpdatedAt.Value > request.Memory.LastPollingTime)
            .Select(e => e.Data)
            .ToList();
        allPublishedEntries.AddRange(updatedPublishedEntries);

        // Remove duplicates
        var distinctEntries = allPublishedEntries
            .GroupBy(e => e.Id)
            .Select(g => g.First())
            .ToList();

        return CreateSuccessResponse(distinctEntries);
    }

    private async Task<PollingEventResponse<DateMemory, SearchEntriesResponse>> ProcessPollingRequestAsync(
        PollingEventRequest<DateMemory> request,
        string state,
        string orderBy,
        Func<EntryDto, bool> filter)
    {
        if (request.Memory == null)
        {
            return CreateInitialResponse();
        }

        var entries = await FetchEntriesAsync(state, orderBy);
        var filteredEntries = entries
            .Where(filter)
            .Select(e => e.Data)
            .ToList();

        return CreateSuccessResponse(filteredEntries);
    }

    private async Task<List<EntryDto>> FetchEntriesAsync(string state, string orderBy)
    {
        var apiRequest = new RestRequest("/api/v1/entries");
        apiRequest.AddParameter("state", state);
        apiRequest.AddParameter("orderBy", orderBy);
        apiRequest.AddParameter("limit", PageLimit);
        apiRequest.AddParameter("offset", 0);

        var response = await Client.ExecuteWithErrorHandling(apiRequest);
        var entriesDto = JsonConvert.DeserializeObject<EntriesDto<EntryDto>>(response.Content ?? string.Empty);
        
        return entriesDto?.Entries ?? new List<EntryDto>();
    }

    private static PollingEventResponse<DateMemory, SearchEntriesResponse> CreateInitialResponse()
    {
        return new()
        {
            Result = null,
            FlyBird = false,
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow
            }
        };
    }

    private static PollingEventResponse<DateMemory, SearchEntriesResponse> CreateSuccessResponse(
        List<EntryResponse> entries)
    {
        return new()
        {
            Result = new SearchEntriesResponse
            {
                Entries = entries,
                TotalCount = entries.Count
            },
            FlyBird = entries.Count > 0,
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow
            }
        };
    }
}