using Apps.Uniform.Models.Dtos.Locales;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Uniform.Handlers;

public class LocaleDataHandler(InvocationContext invocationContext) 
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var apiRequest = new RestRequest("/api/v1/locales");
        var dtoResponse = await Client.ExecuteWithErrorHandling<LocalesDto>(apiRequest);
        
        return dtoResponse.Locales
            .Where(x => context.SearchString == null || x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.Locale, x.Name));
    }
}