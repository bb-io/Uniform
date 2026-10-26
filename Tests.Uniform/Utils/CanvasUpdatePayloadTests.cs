using Apps.Uniform.Utils;
using Newtonsoft.Json.Linq;

namespace Tests.Uniform.Utils;

[TestClass]
public class CanvasUpdatePayloadTests
{
    private static JObject CreatePatternEnvelope() => JObject.Parse(
        """
        {
          "composition": {
            "_id": "5e308708-3a0f-44c5-b300-8a341fb967ce",
            "_name": "Hub Article",
            "type": "page",
            "_locales": ["en-US", "it-IT"],
            "parameters": {
              "title": { "type": "text", "locales": { "en-US": "Hub", "it-IT": "Centro" } }
            }
          },
          "projectId": "fb05a5b2-4a45-49ac-930a-903b19bd186c",
          "state": 0,
          "created": "2025-11-25T16:08:44.441441+00:00",
          "modified": "2026-10-21T22:02:10.685812+00:00",
          "pattern": true,
          "categoryId": "fe6b824c-afc6-48bc-857c-ca350840feb7",
          "workflowId": "5d6a1703-8513-41b0-be33-56a7d8a31376",
          "workflowStageId": "7e50cad6-ed03-42a9-b865-934195f073b4",
          "description": "Shared article layout",
          "previewImageUrl": "https://img.uniform.global/p/hub.png"
        }
        """);

    private static JObject CreateCompositionEnvelope() => JObject.Parse(
        """
        {
          "composition": {
            "_id": "7ceec221-d00b-4da5-9083-3572f29e296f",
            "_name": "/software/confluence/premium",
            "type": "page"
          },
          "projectId": "fb05a5b2-4a45-49ac-930a-903b19bd186c",
          "state": 0,
          "pattern": false,
          "workflowId": "5d6a1703-8513-41b0-be33-56a7d8a31376",
          "workflowStageId": "c94c66af-d12e-49fb-8226-ff59fa86c7e9"
        }
        """);

    [TestMethod]
    public void FromReadEnvelope_Pattern_StaysPattern()
    {
        var payload = CanvasUpdatePayload.FromReadEnvelope(CreatePatternEnvelope());

        Assert.AreEqual(true, payload["pattern"]?.Value<bool>(),
            "PUT /api/v1/canvas replaces the envelope, so a missing 'pattern' demotes the pattern to a composition");
    }

    [TestMethod]
    public void FromReadEnvelope_Pattern_KeepsCategory()
    {
        var payload = CanvasUpdatePayload.FromReadEnvelope(CreatePatternEnvelope());

        Assert.AreEqual("fe6b824c-afc6-48bc-857c-ca350840feb7", payload["categoryId"]?.ToString());
    }

    [TestMethod]
    public void FromReadEnvelope_Pattern_KeepsWorkflowState()
    {
        var payload = CanvasUpdatePayload.FromReadEnvelope(CreatePatternEnvelope());

        Assert.AreEqual("5d6a1703-8513-41b0-be33-56a7d8a31376", payload["workflowId"]?.ToString());
        Assert.AreEqual("7e50cad6-ed03-42a9-b865-934195f073b4", payload["workflowStageId"]?.ToString());
    }

    [TestMethod]
    public void FromReadEnvelope_OrdinaryComposition_KeepsWorkflowState()
    {
        var payload = CanvasUpdatePayload.FromReadEnvelope(CreateCompositionEnvelope());

        Assert.AreEqual("5d6a1703-8513-41b0-be33-56a7d8a31376", payload["workflowId"]?.ToString());
        Assert.AreEqual("c94c66af-d12e-49fb-8226-ff59fa86c7e9", payload["workflowStageId"]?.ToString());
        Assert.AreEqual(false, payload["pattern"]?.Value<bool>());
    }

    [TestMethod]
    public void FromReadEnvelope_EveryEnvelopeField_IsCarriedOver()
    {
        var envelope = CreatePatternEnvelope();

        var payload = CanvasUpdatePayload.FromReadEnvelope(envelope);

        var missingFields = envelope.Properties()
            .Select(property => property.Name)
            .Where(name => !JToken.DeepEquals(envelope[name], payload[name]))
            .ToList();
        Assert.AreEqual(0, missingFields.Count,
            $"Envelope fields lost by the write-back: {string.Join(", ", missingFields)}");
    }

    [TestMethod]
    public void FromReadEnvelope_TranslatedComposition_IsSent()
    {
        var envelope = CreatePatternEnvelope();

        var payload = CanvasUpdatePayload.FromReadEnvelope(envelope);

        Assert.AreEqual("Centro", payload["composition"]!["parameters"]!["title"]!["locales"]!["it-IT"]!.ToString());
    }
}
