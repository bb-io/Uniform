using Apps.Uniform.Api;
using Apps.Uniform.Models.Dtos.Compositions;
using Newtonsoft.Json;
using Tests.Uniform.Base;

namespace Tests.Uniform.Api;

[TestClass]
public class CompositionMetadataListRequestTests : TestBase
{
    private const int MaxMetadataBytesPerComposition = 4 * 1024;

    [TestMethod]
    public async Task Execute_PageWhoseFullContentExceedsUniformResponseLimit_ReturnsCompositionMetadata()
    {
        var client = new Client(InvocationContext.AuthenticationCredentialsProviders.ToList());
        var request = new CompositionMetadataListRequest("0", "updated_at_DESC", 1000, 0);

        var response = await client.ExecuteWithErrorHandling(request);
        var compositions = JsonConvert.DeserializeObject<CompositionsDto<CompositionDto>>(response.Content!)!.Compositions;

        Assert.IsTrue(compositions.Count > 0);
        Assert.IsTrue(compositions.All(composition => !string.IsNullOrEmpty(composition.Data.Id) && !string.IsNullOrEmpty(composition.Data.Name)));
        Assert.IsTrue(response.Content!.Length < compositions.Count * MaxMetadataBytesPerComposition);
    }
}
