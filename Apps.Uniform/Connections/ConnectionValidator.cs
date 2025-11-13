using Apps.Uniform.Api;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.Uniform.Connections;

public class ConnectionValidator: IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new Client(authenticationCredentialsProviders.ToList());
            
            var apiRequest = new RestRequest("/api/v1/project");
            var response = await client.ExecuteWithErrorHandling(apiRequest);
            return new()
            {
                IsValid = response.IsSuccessful,
                Message = response.IsSuccessful ? "Connection successful." : "Connection failed."
            };
        } 
        catch(Exception ex)
        {
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}
