using Newtonsoft.Json.Linq;
using System.Text;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class RichTextToHtmlConverter
{
    private readonly JObject _richTextContent;
    
    public RichTextToHtmlConverter(JObject richTextContent)
    {
        _richTextContent = richTextContent;
    }
    
    public string ToHtml()
    {
        var root = _richTextContent["root"];
        if (root == null)
            return string.Empty;
            
        var children = root["children"] as JArray;
        if (children == null)
            return string.Empty;
            
        var htmlBuilder = new StringBuilder();
        ProcessChildren(children, htmlBuilder);
        
        return htmlBuilder.ToString();
    }
    
    private void ProcessChildren(JArray children, StringBuilder htmlBuilder)
    {
        foreach (var child in children)
        {
            ProcessNode(child as JObject, htmlBuilder);
        }
    }
    
    private void ProcessNode(JObject? node, StringBuilder htmlBuilder)
    {
        if (node == null)
            return;
            
        var nodeType = node["type"]?.ToString();
        
        switch (nodeType)
        {
            case "paragraph":
                ProcessParagraph(node, htmlBuilder);
                break;
            case "text":
                ProcessText(node, htmlBuilder);
                break;
            case "linebreak":
                htmlBuilder.Append("<br>");
                break;
            case "link":
                ProcessLink(node, htmlBuilder);
                break;
            case "heading":
                ProcessHeading(node, htmlBuilder);
                break;
            case "list":
                ProcessList(node, htmlBuilder);
                break;
            case "listitem":
                ProcessListItem(node, htmlBuilder);
                break;
            case "quote":
                ProcessQuote(node, htmlBuilder);
                break;
            case "code":
                ProcessCode(node, htmlBuilder);
                break;
            default:
                // Handle unknown node types by processing children
                var children = node["children"] as JArray;
                if (children != null)
                {
                    ProcessChildren(children, htmlBuilder);
                }
                break;
        }
    }
    
    private void ProcessParagraph(JObject node, StringBuilder htmlBuilder)
    {
        var children = node["children"] as JArray;
        if (children == null || children.Count == 0)
        {
            htmlBuilder.Append("<p></p>");
            return;
        }
        
        htmlBuilder.Append("<p>");
        ProcessChildren(children, htmlBuilder);
        htmlBuilder.Append("</p>");
    }
    
    private void ProcessText(JObject node, StringBuilder htmlBuilder)
    {
        var text = node["text"]?.ToString() ?? string.Empty;
        var format = node["format"]?.ToObject<int>() ?? 0;
        
        var openTags = new StringBuilder();
        var closeTags = new StringBuilder();
        
        // Format flags: 1=bold, 2=italic, 4=strikethrough, 8=underline, 16=code, etc.
        if ((format & 1) != 0) // Bold
        {
            openTags.Append("<strong>");
            closeTags.Insert(0, "</strong>");
        }
        if ((format & 2) != 0) // Italic
        {
            openTags.Append("<i>");
            closeTags.Insert(0, "</i>");
        }
        if ((format & 4) != 0) // Strikethrough
        {
            openTags.Append("<s>");
            closeTags.Insert(0, "</s>");
        }
        if ((format & 8) != 0) // Underline
        {
            openTags.Append("<u>");
            closeTags.Insert(0, "</u>");
        }
        if ((format & 16) != 0) // Code
        {
            openTags.Append("<code>");
            closeTags.Insert(0, "</code>");
        }
        
        htmlBuilder.Append(openTags);
        htmlBuilder.Append(HttpUtility.HtmlEncode(text));
        htmlBuilder.Append(closeTags);
    }
    
    private void ProcessLink(JObject node, StringBuilder htmlBuilder)
    {
        var link = node["link"];
        var path = link?["path"]?.ToString() ?? string.Empty;
        var children = node["children"] as JArray;
        
        htmlBuilder.Append($"<a href=\"{HttpUtility.HtmlAttributeEncode(path)}\">");
        if (children != null)
        {
            ProcessChildren(children, htmlBuilder);
        }
        htmlBuilder.Append("</a>");
    }
    
    private void ProcessHeading(JObject node, StringBuilder htmlBuilder)
    {
        var tag = node["tag"]?.ToString() ?? "h1";
        var children = node["children"] as JArray;
        
        htmlBuilder.Append($"<{tag}>");
        if (children != null)
        {
            ProcessChildren(children, htmlBuilder);
        }
        htmlBuilder.Append($"</{tag}>");
    }
    
    private void ProcessList(JObject node, StringBuilder htmlBuilder)
    {
        var listType = node["listType"]?.ToString();
        var tag = listType == "number" ? "ol" : "ul";
        var children = node["children"] as JArray;
        
        htmlBuilder.Append($"<{tag}>");
        if (children != null)
        {
            ProcessChildren(children, htmlBuilder);
        }
        htmlBuilder.Append($"</{tag}>");
    }
    
    private void ProcessListItem(JObject node, StringBuilder htmlBuilder)
    {
        var children = node["children"] as JArray;
        
        htmlBuilder.Append("<li>");
        if (children != null)
        {
            ProcessChildren(children, htmlBuilder);
        }
        htmlBuilder.Append("</li>");
    }
    
    private void ProcessQuote(JObject node, StringBuilder htmlBuilder)
    {
        var children = node["children"] as JArray;
        
        htmlBuilder.Append("<blockquote>");
        if (children != null)
        {
            ProcessChildren(children, htmlBuilder);
        }
        htmlBuilder.Append("</blockquote>");
    }
    
    private void ProcessCode(JObject node, StringBuilder htmlBuilder)
    {
        var children = node["children"] as JArray;
        
        htmlBuilder.Append("<pre><code>");
        if (children != null)
        {
            ProcessChildren(children, htmlBuilder);
        }
        htmlBuilder.Append("</code></pre>");
    }
}
