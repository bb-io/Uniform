using Apps.Uniform.Utils.Converters;
using Newtonsoft.Json.Linq;

namespace Tests.Uniform.Converters;

/// <summary>
/// Offline round-trip tests for the composition download/upload converters. No API calls.
/// </summary>
[TestClass]
public class CompositionConverterTests
{
    private const string SourceLocale = "en-US";
    private const string TargetLocale = "fr-FR";

    /// <summary>
    /// Mirrors the real shape: parameter ids repeat across components with different types
    /// ('title' is richText on callToAction and text on card), which is what used to break the export.
    /// </summary>
    private static JObject CreateComposition() => JObject.Parse(
        """
        {
          "_id": "26f4c7b3-000d-4c68-84fc-306c198b73e3",
          "_name": "Confluence Product Tour",
          "type": "page",
          "parameters": {
            "pageTitle": { "type": "text", "locales": { "en-US": "Product tour" } },
            "nav": { "type": "select", "value": "main" }
          },
          "slots": {
            "content": [
              {
                "type": "callToAction",
                "parameters": {
                  "title": {
                    "type": "richText",
                    "locales": {
                      "en-US": {
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
                                { "mode": "normal", "text": "Get your team on ", "type": "text", "style": "", "detail": 0, "format": 0, "version": 1 },
                                { "mode": "normal", "text": "Confluence", "type": "text", "style": "", "detail": 0, "format": 2, "version": 1 }
                              ],
                              "direction": null,
                              "textStyle": "",
                              "textFormat": 0
                            }
                          ],
                          "direction": null
                        }
                      },
                      "de-DE": {
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
                                { "mode": "normal", "text": "Confluence für Teams", "type": "text", "style": "", "detail": 0, "format": 0, "version": 1 }
                              ],
                              "direction": null,
                              "textStyle": "",
                              "textFormat": 0
                            }
                          ],
                          "direction": null
                        }
                      }
                    }
                  },
                  "buttonHref": { "type": "link", "value": { "path": "/signup", "type": "url" } }
                },
                "slots": {
                  "treatment": [
                    {
                      "type": "card",
                      "parameters": {
                        "title": { "type": "text", "locales": { "en-US": "Collaborate" } },
                        "description": { "type": "text", "locales": { "en-US": "Work together" } }
                      }
                    }
                  ]
                }
              }
            ]
          }
        }
        """);

    private static string ToHtml(JObject composition) =>
        new CompositionToHtmlConverter(SourceLocale)
            .ToHtml(composition, composition["_id"]!.ToString(), composition["_name"]!.ToString(), "64");

    private static HtmlAgilityPack.HtmlNode SelectParameterDiv(string html, string jsonPath)
    {
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        var div = doc.DocumentNode.SelectSingleNode($"//div[@data-json-path='{jsonPath}']");
        Assert.IsNotNull(div, $"No parameter div for json path '{jsonPath}'");
        return div;
    }

    [TestMethod]
    public void ToHtml_RichTextParameterSharingIdWithTextParameter_RendersMarkupInsteadOfRawJson()
    {
        var html = ToHtml(CreateComposition());

        var div = SelectParameterDiv(html, "slots.content[0].parameters.title.locales.en-US");

        Assert.AreEqual("richText", div.GetAttributeValue("data-parameter-type", ""));
        Assert.AreEqual("<p>Get your team on <i>Confluence</i></p>", div.InnerHtml);

        // The translatable content must not contain the Lexical JSON (data-original-json still holds it)
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);
        var compositionDiv = doc.DocumentNode.SelectSingleNode("//div[@data-composition-id]");
        StringAssert.DoesNotMatch(compositionDiv.InnerHtml, new System.Text.RegularExpressions.Regex("&quot;root&quot;"),
            "Rich text must not be serialized into the HTML as raw JSON");
    }

    [TestMethod]
    public void ToHtml_TextParameters_AreExportedRegardlessOfComponentDefinitions()
    {
        var html = ToHtml(CreateComposition());

        var title = SelectParameterDiv(html, "slots.content[0].slots.treatment[0].parameters.title.locales.en-US");
        Assert.AreEqual("text", title.GetAttributeValue("data-parameter-type", ""));
        Assert.AreEqual("<h2>Collaborate</h2>", title.InnerHtml);

        // Used to be dropped when no localizable definition with that parameter id existed
        var description = SelectParameterDiv(html, "slots.content[0].slots.treatment[0].parameters.description.locales.en-US");
        Assert.AreEqual("<p>Work together</p>", description.InnerHtml);

        var pageTitle = SelectParameterDiv(html, "parameters.pageTitle.locales.en-US");
        Assert.AreEqual("<h2>Product tour</h2>", pageTitle.InnerHtml);
    }

    [TestMethod]
    public void ToHtml_NonTranslatableParameters_AreSkipped()
    {
        var html = ToHtml(CreateComposition());

        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        Assert.IsNull(doc.DocumentNode.SelectSingleNode("//div[@data-parameter-id='nav']"));
        Assert.IsNull(doc.DocumentNode.SelectSingleNode("//div[@data-parameter-id='buttonHref']"));
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_TranslatedRichText_WritesLexicalObjectIntoTargetLocaleOnly()
    {
        var composition = CreateComposition();
        var translatedHtml = ToHtml(composition)
            .Replace("<p>Get your team on <i>Confluence</i></p>", "<p>Réunissez votre équipe sur <i>Confluence</i></p>")
            .Replace("<h2>Collaborate</h2>", "<h2>Collaborez</h2>");

        new HtmlToCompositionConverter().UpdateCompositionFromHtml(translatedHtml, composition, TargetLocale);

        var titleLocales = composition["slots"]!["content"]![0]!["parameters"]!["title"]!["locales"]!;

        var translated = titleLocales[TargetLocale] as JObject;
        Assert.IsNotNull(translated, "Rich text must be restored as a Lexical object, not a string");

        var textNodes = translated["root"]!["children"]![0]!["children"]!;
        Assert.AreEqual("Réunissez votre équipe sur ", textNodes[0]!["text"]!.ToString());
        Assert.AreEqual(0, textNodes[0]!["format"]!.ToObject<int>());
        Assert.AreEqual("Confluence", textNodes[1]!["text"]!.ToString());
        Assert.AreEqual(2, textNodes[1]!["format"]!.ToObject<int>(), "Italic formatting must survive the round trip");

        // Other locales and non-translated parameters stay untouched
        Assert.AreEqual("Get your team on ",
            titleLocales[SourceLocale]!["root"]!["children"]![0]!["children"]![0]!["text"]!.ToString());
        Assert.IsNotNull(titleLocales["de-DE"]);
        Assert.AreEqual("/signup",
            composition["slots"]!["content"]![0]!["parameters"]!["buttonHref"]!["value"]!["path"]!.ToString());

        // Text parameters are still written as plain strings
        var cardTitleLocales = composition["slots"]!["content"]![0]!["slots"]!["treatment"]![0]!["parameters"]!["title"]!["locales"]!;
        Assert.AreEqual(JTokenType.String, cardTitleLocales[TargetLocale]!.Type);
        Assert.AreEqual("Collaborez", cardTitleLocales[TargetLocale]!.ToString());

        Assert.IsTrue(composition["_locales"]!.Any(l => l.ToString() == TargetLocale));
    }

    [TestMethod]
    public void UpdateCompositionFromHtml_FileWithStaleParameterType_StillWritesRichText()
    {
        // Files produced before the fix declare richText parameters as 'text'; the composition
        // itself must win over the attribute, otherwise a plain string corrupts the parameter.
        var composition = CreateComposition();
        var staleHtml = ToHtml(composition).Replace(
            """<div data-parameter-id="title" data-parameter-type="richText" data-json-path="slots.content[0].parameters.title.locales.en-US">""",
            """<div data-parameter-id="title" data-parameter-type="text" data-json-path="slots.content[0].parameters.title.locales.en-US">""");

        new HtmlToCompositionConverter().UpdateCompositionFromHtml(staleHtml, composition, TargetLocale);

        var translated = composition["slots"]!["content"]![0]!["parameters"]!["title"]!["locales"]![TargetLocale] as JObject;
        Assert.IsNotNull(translated);
        Assert.AreEqual("Get your team on ", translated["root"]!["children"]![0]!["children"]![0]!["text"]!.ToString());
    }
}
