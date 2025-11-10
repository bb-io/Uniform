using Apps.Uniform.Models.Dtos.Entries;
using Apps.Uniform.Models.Requests.Entries;
using Apps.Uniform.Models.Responses.Entries;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Uniform.Actions;

[ActionList("Entries")]
public class EntryActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Search entries", Description = "Retrieve a list of entries based on specified criteria")]
    public async Task<SearchEntriesResponse> SearchEntries([ActionParameter] SearchEntryRequest searchEntryRequest)
    {
        var apiRequest = new RestRequest("/api/v1/entries");
        if (searchEntryRequest.State != null)
        {
            apiRequest.AddParameter("state", searchEntryRequest.State);
        }
        
        var dtoResponse = await Client.ExecuteWithErrorHandling<EntriesDto>(apiRequest);
        return new SearchEntriesResponse()
        {
            Entries = dtoResponse.Entries.Select(e => e.Data).ToList(),
            TotalCount = dtoResponse.Entries.Count
        };
    }
}
