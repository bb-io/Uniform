using Apps.Uniform.Events.Models;
using Apps.Uniform.Models.Dtos.Compositions;
using Apps.Uniform.Models.Responses.Compositions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Uniform.Events;

[PollingEventList("Compositions")]
public class CompositionEvents(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const int PageLimit = 150;
    
    [PollingEvent("On compositions created", Description = "Polling event that periodically checks for newly created compositions")]
    public async Task<PollingEventResponse<DateMemory, SearchCompositionsResponse>> OnCompositionsCreatedAsync(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] CompositionFilters filters)
    {
        return await ProcessPollingRequestAsync(
            request,
            filters.State ?? "0",
            "created_at_DESC",
            c => c.CreatedAt > request.Memory!.LastPollingTime);
    }

    [PollingEvent("On compositions updated", Description = "Polling event that periodically checks for updated compositions")]
    public async Task<PollingEventResponse<DateMemory, SearchCompositionsResponse>> OnCompositionsUpdatedAsync(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] CompositionFilters filters)
    {
        return await ProcessPollingRequestAsync(
            request,
            filters.State ?? "0",
            "updated_at_DESC",
            c => c.UpdatedAt.HasValue && c.UpdatedAt.Value > request.Memory!.LastPollingTime);
    }

    private async Task<PollingEventResponse<DateMemory, SearchCompositionsResponse>> ProcessPollingRequestAsync(
        PollingEventRequest<DateMemory> request,
        string state,
        string orderBy,
        Func<CompositionDto, bool> filter)
    {
        if (request.Memory == null)
        {
            return CreateInitialResponse();
        }

        var compositions = await FetchCompositionsAsync(state, orderBy);
        var filteredCompositions = compositions
            .Where(filter)
            .Select(c => c.Data)
            .ToList();

        return CreateSuccessResponse(filteredCompositions);
    }

    private async Task<List<CompositionDto>> FetchCompositionsAsync(string state, string orderBy)
    {
        var apiRequest = new RestRequest("/api/v1/canvas");
        apiRequest.AddParameter("state", state);
        apiRequest.AddParameter("orderBy", orderBy);
        apiRequest.AddParameter("limit", PageLimit);
        apiRequest.AddParameter("offset", 0);

        var response = await Client.ExecuteWithErrorHandling(apiRequest);
        var compositionsDto = JsonConvert.DeserializeObject<CompositionsDto<CompositionDto>>(response.Content ?? string.Empty);
        
        return compositionsDto?.Compositions ?? new List<CompositionDto>();
    }

    private static PollingEventResponse<DateMemory, SearchCompositionsResponse> CreateInitialResponse()
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

    private static PollingEventResponse<DateMemory, SearchCompositionsResponse> CreateSuccessResponse(
        List<CompositionResponse> compositions)
    {
        return new()
        {
            Result = new SearchCompositionsResponse
            {
                Items = compositions,
                TotalCount = compositions.Count
            },
            FlyBird = compositions.Count > 0,
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow
            }
        };
    }
}