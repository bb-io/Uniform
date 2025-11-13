using Apps.Uniform.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Uniform;

public class Invocable : BaseInvocable
{
    protected List<AuthenticationCredentialsProvider> CredentialsProviders =>
        InvocationContext.AuthenticationCredentialsProviders.ToList();

    protected Client Client { get; }
    
    public Invocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new(CredentialsProviders);
    }
}
