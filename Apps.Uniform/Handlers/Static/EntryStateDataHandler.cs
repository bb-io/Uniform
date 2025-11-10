using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Uniform.Handlers.Static;

public class EntryStateDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return new[]
        {
            new DataSourceItem("0", "Draft"),
            new DataSourceItem("64", "Published")
        };
    }
}