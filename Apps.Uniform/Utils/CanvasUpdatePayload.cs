using Newtonsoft.Json.Linq;

namespace Apps.Uniform.Utils;

public static class CanvasUpdatePayload
{
    public static JObject FromReadEnvelope(JObject readEnvelope) =>
        (JObject)readEnvelope.DeepClone();
}
