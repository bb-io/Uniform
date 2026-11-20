using Apps.Uniform.Utils.Converters;
using Apps.Uniform.Utils.Converters.Models;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace Tests.Uniform.Converters;

[TestClass]
public class ExistingTranslationProtectionTests
{
    private const string SourceLocale = "en-US";
    private const string TargetLocale = "de-DE";

    private static JObject RichText(string text) => JObject.Parse(
        $$"""
        {
          "root": {
            "type": "root",
            "format": "",
            "indent": 0,
            "version": 1,
            "children": [
              {
                "type": "paragraph",
                "format": "",
                "indent": 0,
                "version": 1,
                "children": [
                  { "mode": "normal", "text": "{{text}}", "type": "text", "style": "", "detail": 0, "format": 0, "version": 1 }
                ],
                "direction": null
              }
            ],
            "direction": null
          }
        }
        """);

    private static JObject CreateComposition()
    {
        var composition = JObject.Parse(
            """
            {
              "_id": "page-id",
              "_name": "Product page",
              "type": "page",
              "_locales": ["en-US", "de-DE", "fr-FR"],
              "parameters": {
                "heading": { "type": "text", "locales": { "en-US": "Plan every release", "de-DE": "Jede Version planen", "fr-FR": "Planifiez chaque version" } },
                "intro": { "type": "text", "locales": { "en-US": "Welcome" } },
                "summary": { "type": "text", "locales": { "en-US": "Read more", "de-DE": "Mehr lesen (alt)" } },
                "body": { "type": "richText", "locales": {} }
              }
            }
            """);

        var bodyLocales = (JObject)composition["parameters"]!["body"]!["locales"]!;
        bodyLocales[SourceLocale] = RichText("Ship with confidence");
        bodyLocales[TargetLocale] = RichText("Mit Zuversicht ausliefern");
        return composition;
    }

    private static string ToSmartlingFileWithUntranslatedStrings(JObject composition) =>
        new CompositionToHtmlConverter(SourceLocale, new LocalizableParameterSet([]))
            .ToHtml(composition, "page-id", "Product page", "0")
            .Replace("<p>Read more</p>", "<p>Weiterlesen</p>");

    private static JObject Locales(JObject composition, string parameterId) =>
        (JObject)composition["parameters"]![parameterId]!["locales"]!;

    private static void Upload(JObject composition) =>
        HtmlToCompositionConverter.UpdateCompositionFromHtml(ToSmartlingFileWithUntranslatedStrings(composition), composition, TargetLocale);

    [TestMethod]
    public void UpdateCompositionFromHtml_UntranslatedText_KeepsExistingTranslation()
    {
        var composition = CreateComposition();

        Upload(composition);

        Assert.AreEqual("Jede Version planen", Locales(composition, "heading")[TargetLocale]!.ToString(),
            "A string returned in the source language must not replace a translation the locale already has");
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_UntranslatedRichText_KeepsExistingTranslation()
    {
        var composition = CreateComposition();

        Upload(composition);

        Assert.IsTrue(JToken.DeepEquals(RichText("Mit Zuversicht ausliefern"), Locales(composition, "body")[TargetLocale]),
            "Rich text returned in the source language must not replace a translation the locale already has");
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_UntranslatedText_FillsLocaleWithoutTranslation()
    {
        var composition = CreateComposition();

        Upload(composition);

        Assert.AreEqual("Welcome", Locales(composition, "intro")[TargetLocale]?.ToString());
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_DeliveredTranslation_ReplacesExistingTranslation()
    {
        var composition = CreateComposition();

        Upload(composition);

        Assert.AreEqual("Weiterlesen", Locales(composition, "summary")[TargetLocale]!.ToString());
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_UntranslatedText_LeavesOtherLocalesUntouched()
    {
        var composition = CreateComposition();

        Upload(composition);

        Assert.AreEqual("Planifiez chaque version", Locales(composition, "heading")["fr-FR"]!.ToString());
        Assert.AreEqual("Plan every release", Locales(composition, "heading")[SourceLocale]!.ToString());
    }

    [TestMethod]
    public void UpdatePatternFromTranslations_UntranslatedText_KeepsExistingTranslation()
    {
        var pattern = JObject.Parse(
            """
            {
              "_id": "pattern-id",
              "_name": "Footer",
              "type": "footer",
              "_locales": ["en-US", "de-DE"],
              "parameters": { "copyright": { "type": "text", "locales": { "en-US": "All rights reserved", "de-DE": "Alle Rechte vorbehalten" } } }
            }
            """);
        var parameterDiv = HtmlNode.CreateNode(
            """<div data-parameter-id="copyright" data-parameter-type="text"><p>All rights reserved</p></div>""");
        var translation = new PatternTranslation(["parameters", "copyright", "locales", SourceLocale], parameterDiv);

        HtmlToCompositionConverter.UpdatePatternFromTranslations(pattern, [translation], SourceLocale, TargetLocale);

        Assert.AreEqual("Alle Rechte vorbehalten", pattern["parameters"]!["copyright"]!["locales"]![TargetLocale]!.ToString());
    }
}
