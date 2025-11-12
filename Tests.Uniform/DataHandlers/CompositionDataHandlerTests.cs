using Apps.Uniform.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Uniform.Base;

namespace Tests.Uniform.DataHandlers;

[TestClass]
public class CompositionDataHandlerTests : BaseDataHandlerTests
{
    protected override IAsyncDataSourceItemHandler DataHandler => new CompositionDataHandler(InvocationContext);
    protected override string SearchString => "Page";
}