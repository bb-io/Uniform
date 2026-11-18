using System.Net;
using Apps.Uniform.Api;
using Apps.Uniform.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;

namespace Tests.Uniform.Api;

[TestClass]
public class ClientErrorTests
{
    private class ErrorMappingClient() : Client(
    [
        new AuthenticationCredentialsProvider(CredNames.ApiKey, "api-key"),
        new AuthenticationCredentialsProvider(CredNames.ProjectId, "project-id")
    ])
    {
        public Exception Map(RestResponse response) => ConfigureErrorException(response);
    }

    private static RestResponse CreateTooLargeResponse(string? content)
    {
        var request = new RestRequest("/api/v1/canvas", Method.Put)
            .AddStringBody(new string('x', 7 * 1024 * 1024), DataFormat.Json);

        return new RestResponse(request)
        {
            StatusCode = HttpStatusCode.RequestEntityTooLarge,
            Content = content
        };
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("""{"message":"Request Entity Too Large"}""")]
    public void ConfigureErrorException_RequestEntityTooLarge_ExplainsUniformSizeLimit(string? content)
    {
        var exception = new ErrorMappingClient().Map(CreateTooLargeResponse(content));

        Assert.IsInstanceOfType<PluginApplicationException>(exception);
        StringAssert.Contains(exception.Message, "6 MB");
        StringAssert.Contains(exception.Message, "7.0 MB");
    }
}
