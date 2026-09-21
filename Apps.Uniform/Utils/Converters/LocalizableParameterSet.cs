using Apps.Uniform.Models.Dtos.Canvas;

namespace Apps.Uniform.Utils.Converters;

public class LocalizableParameterSet
{
    private readonly HashSet<(string ComponentType, string ParameterId)> _parameters;

    public LocalizableParameterSet(IEnumerable<ComponentDefinitionDto> componentDefinitions)
    {
        _parameters = componentDefinitions
            .SelectMany(definition => definition.Parameters
                .Where(parameter => parameter.Localizable)
                .Select(parameter => (definition.Id, parameter.Id)))
            .ToHashSet();
    }

    public bool Contains(string? componentType, string parameterId) =>
        componentType != null && _parameters.Contains((componentType, parameterId));
}
