using Apps.Uniform.Actions;
using Tests.Uniform.Base;

namespace Tests.Uniform;

[TestClass]
public class EntryActionsTests : TestBase
{
    [TestMethod]
    public async Task SearchEntries_WithDraftEntries_ReturnsNotEmptyList()
    {
        // Arrange
        var actions = new EntryActions(InvocationContext);
        var request = new Apps.Uniform.Models.Requests.Entries.SearchEntryRequest
        {
            State = "0"
        };
        
        // Act
        var response = await actions.SearchEntries(request);
        
        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Entries);

        PrintObject(response);
    }
}
