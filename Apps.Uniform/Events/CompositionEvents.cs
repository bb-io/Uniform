using Apps.Uniform.Api;
using Apps.Uniform.Events.Models;
using Apps.Uniform.Models.Dtos.Compositions;
using Apps.Uniform.Models.Responses.Compositions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Newtonsoft.Json;

namespace Apps.Uniform.Events;

[PollingEventList("Compositions")]
public class CompositionEvents(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const int PageLimit = 150;
    private const string PublishedState = "64";
    
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

    [PollingEvent("On compositions published", Description = "Triggered when published compositions")]
    public async Task<PollingEventResponse<DateMemory, SearchCompositionsResponse>> OnCompositionsPublishedAsync(
         PollingEventRequest<DateMemory> request)
    {
        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Result = null,
                Memory = new DateMemory { LastPollingTime = DateTime.UtcNow }
            };
        }

        var pollStartTime = DateTime.UtcNow;
        var since = request.Memory.LastPollingTime;

        var compositions = await FetchPublishedCompositionsPagedAsync(since);

        var items = compositions
            .Select(c => c.Data)
            .ToList();

        return new()
        {
            Result = new SearchCompositionsResponse
            {
                Items = items,
                TotalCount = items.Count
            },
            FlyBird = items.Count > 0,
            Memory = new DateMemory { LastPollingTime = pollStartTime }
        };
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

        var compositions = await FetchCompositionsPageAsync(state, orderBy, 0);
        var filteredCompositions = compositions
            .Where(filter)
            .Select(c => c.Data)
            .ToList();

        return CreateSuccessResponse(filteredCompositions);
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

    private async Task<List<CompositionDto>> FetchPublishedCompositionsPagedAsync(DateTime since)
    {
        var result = new List<CompositionDto>();
        var offset = 0;

        while (true)
        {
            var page = await FetchCompositionsPageAsync(PublishedState, "updated_at_DESC", offset);
            if (page.Count == 0)
                break;

            foreach (var composition in page)
            {
                if (composition.UpdatedAt.HasValue && composition.UpdatedAt.Value > since)
                {
                    result.Add(composition);
                }
                else
                {
                    return result;
                }
            }

            offset += PageLimit;
        }

        return result;
    }

    private async Task<List<CompositionDto>> FetchCompositionsPageAsync(string state, string orderBy, int offset)
    {
        var apiRequest = new CompositionMetadataListRequest(state, orderBy, PageLimit, offset);

        var response = await Client.ExecuteWithErrorHandling(apiRequest);
        var dto = JsonConvert.DeserializeObject<CompositionsDto<CompositionDto>>(response.Content ?? string.Empty);

        return dto?.Compositions ?? new List<CompositionDto>();
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