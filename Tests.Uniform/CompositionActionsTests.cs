using Apps.Uniform.Actions;
using Tests.Uniform.Base;

namespace Tests.Uniform;

[TestClass]
public class CompositionActionsTests : TestBase
{
    [TestMethod]
    public async Task SearchCompositions_WithPublishedCompositions_ReturnsNotEmptyList()
    {
        // Arrange
        var actions = new CompositionActions(InvocationContext, FileManager);
        var request = new Apps.Uniform.Models.Requests.Compositions.SearchCompositionRequest
        {
            State = "64"
        };
        
        // Act
        var response = await actions.SearchCompositions(request);
        
        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Items);

        PrintObject(response);
    }
    
    [TestMethod]
    public async Task SearchCompositions_WithKeyword_ReturnsFilteredList()
    {
        // Arrange
        var actions = new CompositionActions(InvocationContext, FileManager);
        var request = new Apps.Uniform.Models.Requests.Compositions.SearchCompositionRequest
        {
            State = "64",
            Keyword = "test"
        };
        
        // Act
        var response = await actions.SearchCompositions(request);
        
        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Items);

        PrintObject(response);
    }
    
    [TestMethod]
    public async Task GetComposition_WithValidCompositionId_ReturnsCompositionDetails()
    {
        // Arrange
        var actions = new CompositionActions(InvocationContext, FileManager);
        
        // First, get a composition ID from search
        var searchRequest = new Apps.Uniform.Models.Requests.Compositions.SearchCompositionRequest
        {
            State = "64"
        };
        var searchResponse = await actions.SearchCompositions(searchRequest);
        Assert.IsTrue(searchResponse.Items.Any(), "No compositions found to test get composition");
        
        var firstComposition = searchResponse.Items.First();
        
        var getRequest = new Apps.Uniform.Models.Requests.Compositions.GetCompositionRequest
        {
            CompositionId = firstComposition.Id,
            State = "64"
        };
        
        // Act
        var response = await actions.GetComposition(getRequest);
        
        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(firstComposition.Id, response.Id);
        
        PrintObject(response);
    }
    
    [TestMethod]
    public async Task DeleteComposition_WithDraftState_DeletesDraftVersion()
    {
        // Arrange
        var actions = new CompositionActions(InvocationContext, FileManager);
        var compositionId = "f453ee98-5bb1-4cf1-8cad-0633b105107c";
        var deleteRequest = new Apps.Uniform.Models.Requests.Compositions.DeleteCompositionRequest
        {
            CompositionId = compositionId,
            State = "0"
        };
        
        // Act & Assert (should not throw)
        await actions.DeleteComposition(deleteRequest);
        Console.WriteLine($"Draft composition {compositionId} deleted successfully");
    }
    
    [TestMethod]
    public async Task DownloadComposition_WithValidCompositionId_ReturnsHtmlFile()
    {
        // Arrange
        var actions = new CompositionActions(InvocationContext, FileManager);
        var downloadRequest = new Apps.Uniform.Models.Requests.Compositions.DownloadCompositionRequest
        {
            CompositionId = "f17bfbeb-ed65-4c3b-8175-9149e61a6472",
            Locale = "en-US",
            State = "0"
        };
        
        // Act
        var response = await actions.DownloadComposition(downloadRequest);
        
        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Content);
        Assert.IsTrue(response.Content.Name.EndsWith(".html"));
        
        PrintObject(response);
    }
    
    [TestMethod]
    public async Task UploadComposition_WithValidHtmlFile_UpdatesComposition()
    {
        // Arrange
        var actions = new CompositionActions(InvocationContext, FileManager);
        var uploadRequest = new Apps.Uniform.Models.Requests.Compositions.UploadCompositionRequest
        {
            Content = new()
            {
                Name = "Test static composition_en-US_fr-FR.html",
                ContentType = "text/html",
            },
            Locale = "fr-FR",
            //CompositionId = ""
        };
        
        // Act & Assert (should not throw)
        await actions.UploadComposition(uploadRequest);
        Console.WriteLine("Composition uploaded successfully");
    }

    [TestMethod]
    public async Task Get_Composition_Returns_Value()
    {
        var actions = new CompositionActions(InvocationContext, FileManager);
        var compositionId = "b4fa7a4c-427a-403a-a255-8f37bb211d17";
        var deleteRequest = new Apps.Uniform.Models.Requests.Compositions.GetCompositionRequest
        {
            CompositionId = compositionId,
            State = "64"
        };

        var response = await actions.GetComposition(deleteRequest);
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(response));
        Assert.IsNotNull(response);
    }
}
