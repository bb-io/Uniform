using Apps.Uniform.Extensions;
using Newtonsoft.Json.Linq;

namespace Apps.Uniform.Utils.Converters;

public class PatternOverrides(JObject resolvedComposition, JObject rawComposition)
{
    private const char OverrideKeySeparator = '|';
    private const int LocaleSuffixLength = 2;

    private JObject? Overrides => rawComposition["_overrides"] as JObject;

    public JObject? FindParameter(List<string> pathParts) =>
        FindParameterOverride(pathParts) ?? FindSlotOverrideParameter(pathParts);

    private JObject? FindParameterOverride(List<string> pathParts)
    {
        var parameterContainerIndex = pathParts.FindLastIndex(part => part is "parameters" or "fields");
        if (parameterContainerIndex < 0 || parameterContainerIndex + 1 >= pathParts.Count)
            return null;

        var componentId = resolvedComposition.Walk(pathParts.Take(parameterContainerIndex))?["_id"]?.ToString();
        if (string.IsNullOrWhiteSpace(componentId))
            return null;

        var parameterContainer = pathParts[parameterContainerIndex];
        var parameterId = pathParts[parameterContainerIndex + 1];

        return Overrides?[componentId]?[parameterContainer]?[parameterId] as JObject;
    }

    private JObject? FindSlotOverrideParameter(List<string> pathParts)
    {
        string? instanceId = null;
        JObject? overrideItem = null;
        var relativeStart = 0;
        JToken? current = resolvedComposition;

        for (var i = 0; i < pathParts.Count - LocaleSuffixLength && current != null; i++)
        {
            current = current.Step(pathParts[i]);
            if (current is not JObject node || node["_id"]?.ToString() is not { Length: > 0 } componentId)
                continue;

            if (instanceId != null && FindSlotItem(instanceId, componentId) is { } slotItem)
            {
                overrideItem = slotItem;
                relativeStart = i + 1;
            }

            if (node["_pattern"]?.ToString() is { Length: > 0 })
                instanceId = componentId;
        }

        return overrideItem?.Walk(pathParts.Skip(relativeStart).SkipLast(LocaleSuffixLength)) as JObject;
    }

    private JObject? FindSlotItem(string instanceId, string componentId) =>
        OverridesOwnedBy(instanceId)
            .SelectMany(entry => SlotComponents(entry["slots"]))
            .FirstOrDefault(component => component["_id"]?.ToString() == componentId);

    private IEnumerable<JObject> OverridesOwnedBy(string instanceId) =>
        Overrides?.Properties()
            .Where(entry => entry.Name == instanceId || entry.Name.StartsWith($"{instanceId}{OverrideKeySeparator}"))
            .Select(entry => entry.Value)
            .OfType<JObject>()
        ?? [];

    private static IEnumerable<JObject> SlotComponents(JToken? slots) =>
        (slots as JObject)?.Properties()
            .Select(slot => slot.Value)
            .OfType<JArray>()
            .SelectMany(slot => slot.OfType<JObject>())
            .SelectMany(component => SlotComponents(component["slots"]).Prepend(component))
        ?? [];
}
