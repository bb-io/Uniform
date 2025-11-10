using Apps.Uniform.Actions;
using Tests.Uniform.Base;

namespace Tests.Uniform;

[TestClass]
public class ActionTests : TestBase
{
    [TestMethod]
    public async Task Dynamic_handler_works()
    {
        var actions = new Actions(InvocationContext);

        await actions.Action();
    }
}
