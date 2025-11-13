using Apps.Uniform.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Uniform.Base;

namespace Tests.Uniform.DataHandlers;

[TestClass]
public class EntryDataHandlerTests : BaseDataHandlerTests
{
    protected override IAsyncDataSourceItemHandler DataHandler => new EntryDataHandler(InvocationContext);
    protected override string SearchString => "Hobbit";
}