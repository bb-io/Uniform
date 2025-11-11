using Apps.Uniform.Models.Dtos.Entries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Uniform.Handlers;

public class EntryDataHandler(InvocationContext invocationContext) 
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var apiRequest = new RestRequest("/api/v1/entries")
            .AddParameter("state", "0");

        if (context.SearchString != null)
        {
            apiRequest.AddParameter("keyword", context.SearchString);
        }
        
        var dtoResponse = await Client.AutoPaginateAsync<EntryDto>(apiRequest, json =>
            JsonConvert.DeserializeObject<EntriesDto<EntryDto>>(json)?.Entries ?? []);
        
        return dtoResponse
            .Where(x => context.SearchString == null || x.Data.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.Data.Id, x.Data.Name));
    }
}