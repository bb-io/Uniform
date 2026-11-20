using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Apps.Uniform.Utils.Converters;

public static class UntranslatedValueDetector
{
    private static readonly Regex WhitespaceRun = new(@"\s+", RegexOptions.Compiled);

    public static bool ReplacesTranslationWithSource(
        JObject parameterObject,
        string sourceLocale,
        string targetLocale,
        JToken incomingValue)
    {
        if (sourceLocale == targetLocale || parameterObject["locales"] is not JObject locales)
            return false;

        var sourceText = ToPlainText(locales[sourceLocale]);
        var existingText = ToPlainText(locales[targetLocale]);

        return sourceText.Length > 0 &&
               existingText.Length > 0 &&
               existingText != sourceText &&
               ToPlainText(incomingValue) == sourceText;
    }

    private static string ToPlainText(JToken? value) => value switch
    {
        JValue { Type: JTokenType.String } text => Normalize(text.ToString()),
        JContainer richText => Normalize(string.Concat(RichTextFragments(richText))),
        _ => string.Empty
    };

    private static IEnumerable<string> RichTextFragments(JContainer richText) =>
        richText.Descendants()
            .OfType<JProperty>()
            .Where(property => property is { Name: "text", Value.Type: JTokenType.String })
            .Select(property => property.Value.ToString());

    private static string Normalize(string text) => WhitespaceRun.Replace(text, " ").Trim();
}
