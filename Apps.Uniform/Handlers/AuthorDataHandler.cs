using Apps.Uniform.Models.Dtos.Compositions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Uniform.Handlers;

public class AuthorDataHandler(InvocationContext invocationContext)
    : CompositionMetadataDataHandler(invocationContext)
{
    protected override string FilterField => "author";

    protected override string? GetValue(CompositionDto composition) => composition.Author;
}
