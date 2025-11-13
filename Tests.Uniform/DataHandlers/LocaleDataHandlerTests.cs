using Apps.Uniform.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Uniform.Base;

namespace Tests.Uniform.DataHandlers;

[TestClass]
public class LocaleDataHandlerTests : BaseDataHandlerTests
{
    protected override IAsyncDataSourceItemHandler DataHandler => new LocaleDataHandler(InvocationContext);
    protected override string SearchString => "Fr";
}