using Apps.Uniform.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Uniform.Api;

public class Client : BlackBirdRestClient
{
    private readonly List<AuthenticationCredentialsProvider> _credentialsProviders;
    
    public Client(List<AuthenticationCredentialsProvider> credentialsProviders) : base(new()
    {
        BaseUrl = new Uri("https://uniform.app"),
    })
    {
        var apiKeyCredentialProvider = credentialsProviders.Get(CredNames.ApiKey);
        this.AddDefaultHeader("x-api-key", apiKeyCredentialProvider.Value);
        _credentialsProviders = credentialsProviders;
    }

    public override Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        if (!request.Parameters.Any(x => x.Type == ParameterType.QueryString && x.Name == "projectId"))
        {
            var credentialProvider = _credentialsProviders.Get(CredNames.ProjectId);
            request.AddQueryParameter("projectId", credentialProvider.Value);
        }
        
        return base.ExecuteWithErrorHandling(request);
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        if (string.IsNullOrEmpty(response.Content))
        {
            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                return new PluginApplicationException($"Request failed with status code {response.StatusCode}, {response.StatusDescription}");
            }
            
            return new PluginApplicationException(response.ErrorMessage);
        }
        
        if(response.ContentType == "text/plain")
        {
            return new PluginApplicationException(response.Content);
        }
        
        var errorDto = JsonConvert.DeserializeObject<Models.Dtos.ErrorDto>(response.Content);
        return new PluginApplicationException(string.Join(", ", errorDto?.ErrorMessage ?? new List<string> { $"Request failed with status code {response.StatusCode}, {response.StatusDescription}" }));
    }
}