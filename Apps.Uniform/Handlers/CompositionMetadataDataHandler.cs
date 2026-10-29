using Apps.Uniform.Models.Dtos.Compositions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Uniform.Handlers;

public abstract class CompositionMetadataDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    protected abstract string FilterField { get; }

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest("/api/v1/canvas")
            .AddQueryParameter("state", "0")
            .AddQueryParameter("limit", "1")
            .AddQueryParameter("facetBy", FilterField);

        var response = await Client.ExecuteWithErrorHandling(request);
        var dto = JsonConvert.DeserializeObject<CompositionsDto<CompositionDto>>(response.Content ?? string.Empty);
        IEnumerable<string> values = dto?.Facets.GetValueOrDefault(FilterField)?.Keys
            ?? Enumerable.Empty<string>();

        return values
            .Where(x => context.SearchString == null ||
                        x.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(x => new DataSourceItem(x, x));
    }
}
