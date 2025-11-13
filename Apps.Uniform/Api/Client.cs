using Apps.Uniform.Constants;
using Apps.Uniform.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        if (request.Method == Method.Get && !request.Parameters.Any(x => x.Type == ParameterType.QueryString && x.Name == "projectId"))
        {
            var credentialProvider = _credentialsProviders.Get(CredNames.ProjectId);
            request.AddQueryParameter("projectId", credentialProvider.Value);
        }
        
        return base.ExecuteWithErrorHandling(request);
    }

    public async Task<List<T>> AutoPaginateAsync<T>(RestRequest request, Func<string, List<T>> mapFunction)
    {
        const int limit = 100;
        int offset = 0;
        
        var allItems = new List<T>();
        while (true)
        {
            request.AddOrUpdateParameter("limit", limit);
            request.AddOrUpdateParameter("offset", offset);

            var response = await ExecuteWithErrorHandling(request);
            var content = response.Content ?? string.Empty;
            var items = mapFunction.Invoke(content);
            
            allItems.AddRange(items);
            if (items.Count < limit)
            {
                break;
            }

            offset += limit;
        }
        
        return allItems;
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
        
        var errorJObject = JsonConvert.DeserializeObject<JObject>(response.Content);
        var errorMessage = string.Empty;
        if(errorJObject!["errorMessage"] != null)
        {
            if(errorJObject["errorMessage"]!.Type == JTokenType.Array)
            {
                var messages = errorJObject["errorMessage"]!.ToObject<List<string>>();
                errorMessage = string.Join(", ", messages ?? new List<string>());
            }
            else if(errorJObject["errorMessage"]!.Type == JTokenType.String)
            {
                errorMessage = errorJObject["errorMessage"]!.ToString() ?? string.Empty;
            }
        }
        
        if(string.IsNullOrEmpty(errorMessage))
        {
            errorMessage = $"Request failed with status code {response.StatusCode}, {response.StatusDescription}";
        }
        
        return new PluginApplicationException(errorMessage);
    }
}