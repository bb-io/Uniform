using Apps.Uniform.Models.Dtos.Canvas;
using Apps.Uniform.Utils.Converters;
using Apps.Uniform.Utils.Converters.Models;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace Tests.Uniform.Converters;

[TestClass]
public class MissingLocaleFallbackTests
{
    private const string SourceLocale = "en-US";
    private const string TargetLocale = "fr-FR";
    private static readonly string[] PendingLocales = ["de-DE", "ja-JP"];

    private static JObject CreateComposition() => JObject.Parse(
        """
        {
          "_id": "page-id",
          "_name": "Resources",
          "type": "page",
          "_locales": ["en-US", "fr-FR", "de-DE", "ja-JP"],
          "parameters": {
            "heading": { "type": "text", "value": "Get started" },
            "intro": { "type": "text", "locales": { "en-US": "Hello", "de-DE": "Hallo" } },
            "body": {
              "type": "richText",
              "value": {
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
                        { "mode": "normal", "text": "Body text", "type": "text", "style": "", "detail": 0, "format": 0, "version": 1 }
                      ],
                      "direction": null
                    }
                  ],
                  "direction": null
                }
              }
            },
            "theme": { "type": "select", "value": "dark" }
          }
        }
        """);

    private static string ToTranslatedHtml(JObject composition)
    {
        var definitions = new LocalizableParameterSet(
        [
            new ComponentDefinitionDto
            {
                Id = "page",
                Parameters =
                [
                    new ParameterDefinitionDto { Id = "heading", Localizable = true },
                    new ParameterDefinitionDto { Id = "body", Localizable = true }
                ]
            }
        ]);

        return new CompositionToHtmlConverter(SourceLocale, definitions)
            .ToHtml(composition, "page-id", "Resources", "0")
            .Replace("<h2>Get started</h2>", "<h2>Commencer</h2>")
            .Replace("<p>Hello</p>", "<p>Bonjour</p>")
            .Replace("<p>Body text</p>", "<p>Texte</p>");
    }

    private static JObject Parameter(JObject composition, string parameterId) =>
        (JObject)composition["parameters"]![parameterId]!;

    [TestMethod]
    public void UpdateCompositionFromHtml_ParameterWithoutLocales_FillsPendingDeclaredLocalesWithSourceValue()
    {
        var composition = CreateComposition();

        HtmlToCompositionConverter.UpdateCompositionFromHtml(ToTranslatedHtml(composition), composition, TargetLocale);

        var headingLocales = Parameter(composition, "heading")["locales"]!;
        Assert.AreEqual("Commencer", headingLocales[TargetLocale]!.ToString());
        Assert.AreEqual("Get started", headingLocales[SourceLocale]!.ToString());
        foreach (var locale in PendingLocales)
            Assert.AreEqual("Get started", headingLocales[locale]?.ToString(), $"'{locale}' must default to the source value");
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_RichTextWithoutLocales_FillsPendingDeclaredLocalesWithIndependentSourceCopies()
    {
        var composition = CreateComposition();

        HtmlToCompositionConverter.UpdateCompositionFromHtml(ToTranslatedHtml(composition), composition, TargetLocale);

        var bodyLocales = (JObject)Parameter(composition, "body")["locales"]!;
        Assert.AreEqual("Texte", bodyLocales[TargetLocale]!.SelectToken("root.children[0].children[0].text")!.ToString());
        foreach (var locale in PendingLocales)
        {
            Assert.IsTrue(JToken.DeepEquals(bodyLocales[SourceLocale], bodyLocales[locale]), $"'{locale}' must default to the source value");
            Assert.AreNotSame(bodyLocales[SourceLocale], bodyLocales[locale]);
        }
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_LocalizedParameter_FillsOnlyLocalesWithoutValue()
    {
        var composition = CreateComposition();

        HtmlToCompositionConverter.UpdateCompositionFromHtml(ToTranslatedHtml(composition), composition, TargetLocale);

        var introLocales = Parameter(composition, "intro")["locales"]!;
        Assert.AreEqual("Bonjour", introLocales[TargetLocale]!.ToString());
        Assert.AreEqual("Hallo", introLocales["de-DE"]!.ToString(), "An existing translation must never be overwritten");
        Assert.AreEqual("Hello", introLocales["ja-JP"]?.ToString());
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_ParameterNotInFile_IsLeftUntouched()
    {
        var composition = CreateComposition();
        var theme = Parameter(composition, "theme").DeepClone();

        HtmlToCompositionConverter.UpdateCompositionFromHtml(ToTranslatedHtml(composition), composition, TargetLocale);

        Assert.IsTrue(JToken.DeepEquals(theme, Parameter(composition, "theme")));
    }

    [TestMethod]
    public void UpdatePatternFromTranslations_ParameterWithoutLocales_FillsLocalesDeclaredOnPattern()
    {
        var pattern = JObject.Parse(
            """
            {
              "_id": "pattern-id",
              "_name": "Footer",
              "type": "footer",
              "_locales": ["en-US", "ja-JP"],
              "parameters": { "copyright": { "type": "text", "value": "All rights reserved" } }
            }
            """);
        var parameterDiv = HtmlNode.CreateNode(
            """<div data-parameter-id="copyright" data-parameter-type="text"><p>Tous droits réservés</p></div>""");
        var translation = new PatternTranslation(["parameters", "copyright", "locales", SourceLocale], parameterDiv);

        HtmlToCompositionConverter.UpdatePatternFromTranslations(pattern, [translation], SourceLocale, TargetLocale);

        var copyrightLocales = pattern["parameters"]!["copyright"]!["locales"]!;
        Assert.AreEqual("Tous droits réservés", copyrightLocales[TargetLocale]!.ToString());
        Assert.AreEqual("All rights reserved", copyrightLocales["ja-JP"]?.ToString());
    }
}
