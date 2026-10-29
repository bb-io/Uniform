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

    protected abstract string? GetValue(CompositionDto composition);

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new RestRequest("/api/v1/canvas");

        if (!string.IsNullOrWhiteSpace(context.SearchString))
        {
            request.AddQueryParameter($"filters.{FilterField}[match]", context.SearchString);
        }

        var compositions = await Client.AutoPaginateAsync<CompositionDto>(request, json =>
            JsonConvert.DeserializeObject<CompositionsDto<CompositionDto>>(json)?.Compositions ?? []);

        return compositions
            .Select(GetValue)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Where(x => context.SearchString == null ||
                        x.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(x => new DataSourceItem(x, x));
    }
}
