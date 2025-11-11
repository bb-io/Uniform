using Apps.Uniform.Events;
using Apps.Uniform.Events.Models;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Uniform.Base;

namespace Tests.Uniform;

[TestClass]
public class EntryEventsTests : TestBase
{
    [TestMethod]
    public async Task OnEntriesCreated_FirstCall_ReturnsMemory()
    {
        // Arrange
        var events = new EntryEvents(InvocationContext);
        var request = new PollingEventRequest<DateMemory>
        {
            Memory = null
        };
        var filters = new EntryFilters
        {
            State = "0"
        };

        // Act
        var response = await events.OnEntriesCreatedAsync(request, filters);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Memory);
        Assert.IsFalse(response.FlyBird);
        Assert.IsNull(response.Result);
        
        PrintObject(response);
    }

    [TestMethod]
    public async Task OnEntriesCreated_WithMemory_ReturnsNewEntries()
    {
        // Arrange
        var events = new EntryEvents(InvocationContext);
        var request = new PollingEventRequest<DateMemory>
        {
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow.AddDays(-7)
            }
        };
        var filters = new EntryFilters
        {
            State = "0"
        };

        // Act
        var response = await events.OnEntriesCreatedAsync(request, filters);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        
        PrintObject(response);
        Console.WriteLine($"Found {response.Result.TotalCount} new entries");
    }

    [TestMethod]
    public async Task OnEntriesUpdated_WithMemory_ReturnsUpdatedEntries()
    {
        // Arrange
        var events = new EntryEvents(InvocationContext);
        var request = new PollingEventRequest<DateMemory>
        {
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow.AddDays(-7)
            }
        };
        var filters = new EntryFilters
        {
            State = "0"
        };

        // Act
        var response = await events.OnEntriesUpdatedAsync(request, filters);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        
        PrintObject(response);
        Console.WriteLine($"Found {response.Result.TotalCount} updated entries");
    }

    [TestMethod]
    public async Task OnEntriesPublished_WithMemory_ReturnsPublishedEntries()
    {
        // Arrange
        var events = new EntryEvents(InvocationContext);
        var request = new PollingEventRequest<DateMemory>
        {
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow.AddDays(-30)
            }
        };

        // Act
        var response = await events.OnEntriesPublishedAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        
        PrintObject(response);
        Console.WriteLine($"Found {response.Result.TotalCount} published entries");
    }

    [TestMethod]
    public async Task OnEntriesCreated_WithDefaultState_UsesDraft()
    {
        // Arrange
        var events = new EntryEvents(InvocationContext);
        var request = new PollingEventRequest<DateMemory>
        {
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow.AddDays(-7)
            }
        };
        var filters = new EntryFilters(); // No state specified, should default to "0"

        // Act
        var response = await events.OnEntriesCreatedAsync(request, filters);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Memory);
        Assert.IsNotNull(response.Result);
        
        PrintObject(response);
        Console.WriteLine($"Found {response.Result.TotalCount} new draft entries");
    }
}
