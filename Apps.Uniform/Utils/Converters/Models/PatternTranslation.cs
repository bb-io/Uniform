using HtmlAgilityPack;

namespace Apps.Uniform.Utils.Converters.Models;

public record PatternTranslation(List<string> PathParts, HtmlNode ParameterDiv);