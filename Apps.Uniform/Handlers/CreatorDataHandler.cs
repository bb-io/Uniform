using Apps.Uniform.Models.Dtos.Compositions;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Uniform.Handlers;

public class CreatorDataHandler(InvocationContext invocationContext)
    : CompositionMetadataDataHandler(invocationContext)
{
    protected override string FilterField => "creator";

    protected override string? GetValue(CompositionDto composition) => composition.Creator;
}
