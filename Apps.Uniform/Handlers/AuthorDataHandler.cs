using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Uniform.Handlers;

public class AuthorDataHandler(InvocationContext invocationContext)
    : CompositionMetadataDataHandler(invocationContext)
{
    protected override string FilterField => "author";
}
