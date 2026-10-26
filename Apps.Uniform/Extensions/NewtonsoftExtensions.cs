using Newtonsoft.Json.Linq;

namespace Apps.Uniform.Extensions;

public static class NewtonsoftExtensions
{
    public static JToken? Step(this JToken current, string part)
    {
        if (!part.StartsWith('[') || !part.EndsWith(']'))
            return current is JObject obj ? obj[part] : null;

        var index = int.Parse(part[1..^1]);
        return current is JArray array && index < array.Count ? array[index] : null;
    }

    public static JToken? Walk(this JToken root, IEnumerable<string> parts)
    {
        JToken? current = root;
        foreach (var part in parts)
        {
            current = current?.Step(part);
            if (current == null)
                return null;
        }

        return current;
    }
}
