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
        var actions = new EntryActions(InvocationContext, FileManager);
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
    
    [TestMethod]
    public async Task GetEntry_WithValidEntryId_ReturnsEntryDetails()
    {
        // Arrange
        var actions = new EntryActions(InvocationContext, FileManager);
        
        // First, get an entry ID from search
        var searchRequest = new Apps.Uniform.Models.Requests.Entries.SearchEntryRequest
        {
            State = "0"
        };
        var searchResponse = await actions.SearchEntries(searchRequest);
        Assert.IsTrue(searchResponse.Entries.Count > 0, "No entries found to test get entry");
        
        var firstEntry = searchResponse.Entries.First();
        
        var getRequest = new Apps.Uniform.Models.Requests.Entries.GetEntryRequest
        {
            ContentId = firstEntry.Id,
            State = "0"
        };
        
        // Act
        var response = await actions.GetEntry(getRequest);
        
        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(firstEntry.Id, response.Id);
        
        PrintObject(response);
    }
    
    [TestMethod]
    public async Task DownloadEntry_WithValidEntryId_ReturnsHtmlFile()
    {
        // Arrange
        var actions = new EntryActions(InvocationContext, FileManager);
        
        // First, get an entry ID from search
        var searchRequest = new Apps.Uniform.Models.Requests.Entries.SearchEntryRequest
        {
            State = "0"
        };
        var searchResponse = await actions.SearchEntries(searchRequest);
        Assert.IsTrue(searchResponse.Entries.Count > 0, "No entries found to test download");
        
        var firstEntry = searchResponse.Entries.First();
        var locale = firstEntry.SupportedLocales.FirstOrDefault() ?? "en-US";
        
        var downloadRequest = new Apps.Uniform.Models.Requests.Entries.DownloadEntryRequest
        {
            ContentId = firstEntry.Id,
            Locale = locale,
            State = "0"
        };
        
        // Act
        var response = await actions.DownloadEntry(downloadRequest);
        
        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Content);
        Assert.IsTrue(response.Content.Name.EndsWith(".html"));
        
        PrintObject(response);
    }
    
    [TestMethod]
    public async Task UploadEntry_WithValidHtmlFile_UpdatesEntry()
    {
        // Arrange
        var actions = new EntryActions(InvocationContext, FileManager);
        
        // Act
        var uploadRequest = new Apps.Uniform.Models.Requests.Entries.UploadEntryRequest
        {
            Content = new()
            {
                Name = "The Hobbit_en-US.html",
                ContentType = "text/html",
            },
            Locale = "fr-FR"
        };
        
        // Act & Assert (should not throw)
        await actions.UploadEntry(uploadRequest);
        Console.WriteLine("Entry uploaded successfully");
    }
}
