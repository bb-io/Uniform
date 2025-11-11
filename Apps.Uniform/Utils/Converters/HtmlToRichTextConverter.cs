using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Apps.Uniform.Utils.Converters;

public class HtmlToRichTextConverter
{
    public JObject ToRichText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        var root = new JObject
        {
            ["type"] = "root",
            ["format"] = "",
            ["indent"] = 0,
            ["version"] = 1,
            ["children"] = new JArray(),
            ["direction"] = "ltr"
        };
        
        ProcessChildNodes(doc.DocumentNode.ChildNodes, root["children"] as JArray);
        
        return new JObject { ["root"] = root };
    }
    
    private void ProcessChildNodes(HtmlNodeCollection nodes, JArray? children)
    {
        if (children == null || nodes == null)
            return;
            
        foreach (var node in nodes)
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                var lexicalNode = ConvertHtmlNodeToLexical(node);
                if (lexicalNode != null)
                {
                    children.Add(lexicalNode);
                }
            }
            else if (node.NodeType == HtmlNodeType.Text)
            {
                var text = HttpUtility.HtmlDecode(node.InnerText);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    children.Add(CreateTextNode(text, 0));
                }
            }
        }
    }
    
    private JObject? ConvertHtmlNodeToLexical(HtmlNode node)
    {
        switch (node.Name.ToLower())
        {
            case "p":
                return CreateParagraphNode(node);
            case "br":
                return CreateLinebreakNode();
            case "h1":
            case "h2":
            case "h3":
            case "h4":
            case "h5":
            case "h6":
                return CreateHeadingNode(node);
            case "a":
                return CreateLinkNode(node);
            case "ul":
                return CreateListNode(node, "bullet");
            case "ol":
                return CreateListNode(node, "number");
            case "li":
                return CreateListItemNode(node);
            case "blockquote":
                return CreateQuoteNode(node);
            case "pre":
                var codeNode = node.SelectSingleNode(".//code");
                if (codeNode != null)
                    return CreateCodeNode(codeNode);
                return CreateParagraphNode(node);
            case "strong":
            case "b":
            case "i":
            case "em":
            case "u":
            case "s":
            case "strike":
            case "code":
                // These are formatting tags, handled in text processing
                return null;
            default:
                // For unknown tags, try to process children
                var paragraph = CreateParagraphNode(node);
                return paragraph;
        }
    }
    
    private JObject CreateParagraphNode(HtmlNode node)
    {
        var paragraph = new JObject
        {
            ["type"] = "paragraph",
            ["format"] = "",
            ["indent"] = 0,
            ["version"] = 1,
            ["children"] = new JArray(),
            ["direction"] = "ltr"
        };
        
        ProcessInlineContent(node, paragraph["children"] as JArray);
        
        // Ensure paragraph always has at least one child
        var children = paragraph["children"] as JArray;
        if (children != null && children.Count == 0)
        {
            children.Add(CreateTextNode("", 0));
        }
        
        return paragraph;
    }
    
    private void ProcessInlineContent(HtmlNode node, JArray? children)
    {
        if (children == null)
            return;
            
        foreach (var child in node.ChildNodes)
        {
            if (child.NodeType == HtmlNodeType.Text)
            {
                var text = HttpUtility.HtmlDecode(child.InnerText);
                if (!string.IsNullOrEmpty(text))
                {
                    var format = GetFormatFromParents(child);
                    children.Add(CreateTextNode(text, format));
                }
            }
            else if (child.NodeType == HtmlNodeType.Element)
            {
                switch (child.Name.ToLower())
                {
                    case "br":
                        children.Add(CreateLinebreakNode());
                        break;
                    case "a":
                        children.Add(CreateLinkNode(child));
                        break;
                    case "strong":
                    case "b":
                    case "i":
                    case "em":
                    case "u":
                    case "s":
                    case "strike":
                    case "code":
                        ProcessInlineContent(child, children);
                        break;
                    default:
                        ProcessInlineContent(child, children);
                        break;
                }
            }
        }
    }
    
    private JObject CreateTextNode(string text, int format)
    {
        return new JObject
        {
            ["mode"] = "normal",
            ["text"] = text,
            ["type"] = "text",
            ["style"] = "",
            ["detail"] = 0,
            ["format"] = format,
            ["version"] = 1
        };
    }
    
    private JObject CreateLinebreakNode()
    {
        return new JObject
        {
            ["type"] = "linebreak",
            ["version"] = 1
        };
    }
    
    private JObject CreateLinkNode(HtmlNode node)
    {
        var href = node.GetAttributeValue("href", "");
        
        var link = new JObject
        {
            ["link"] = new JObject
            {
                ["path"] = href,
                ["type"] = "url"
            },
            ["type"] = "link",
            ["format"] = "",
            ["indent"] = 0,
            ["version"] = 1,
            ["children"] = new JArray(),
            ["direction"] = "ltr"
        };
        
        ProcessInlineContent(node, link["children"] as JArray);
        
        return link;
    }
    
    private JObject CreateHeadingNode(HtmlNode node)
    {
        var level = int.Parse(node.Name.Substring(1));
        
        var heading = new JObject
        {
            ["type"] = "heading",
            ["tag"] = node.Name.ToLower(),
            ["format"] = "",
            ["indent"] = 0,
            ["version"] = 1,
            ["children"] = new JArray(),
            ["direction"] = "ltr"
        };
        
        ProcessInlineContent(node, heading["children"] as JArray);
        
        return heading;
    }
    
    private JObject CreateListNode(HtmlNode node, string listType)
    {
        var list = new JObject
        {
            ["type"] = "list",
            ["listType"] = listType,
            ["start"] = 1,
            ["tag"] = node.Name.ToLower(),
            ["format"] = "",
            ["indent"] = 0,
            ["version"] = 1,
            ["children"] = new JArray(),
            ["direction"] = "ltr"
        };
        
        var children = list["children"] as JArray;
        foreach (var li in node.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
        {
            var listItem = CreateListItemNode(li);
            if (listItem != null)
            {
                children?.Add(listItem);
            }
        }
        
        return list;
    }
    
    private JObject CreateListItemNode(HtmlNode node)
    {
        var listItem = new JObject
        {
            ["type"] = "listitem",
            ["value"] = 1,
            ["format"] = "",
            ["indent"] = 0,
            ["version"] = 1,
            ["children"] = new JArray(),
            ["direction"] = "ltr"
        };
        
        ProcessInlineContent(node, listItem["children"] as JArray);
        
        return listItem;
    }
    
    private JObject CreateQuoteNode(HtmlNode node)
    {
        var quote = new JObject
        {
            ["type"] = "quote",
            ["format"] = "",
            ["indent"] = 0,
            ["version"] = 1,
            ["children"] = new JArray(),
            ["direction"] = "ltr"
        };
        
        ProcessChildNodes(node.ChildNodes, quote["children"] as JArray);
        
        return quote;
    }
    
    private JObject CreateCodeNode(HtmlNode node)
    {
        var code = new JObject
        {
            ["type"] = "code",
            ["language"] = null,
            ["format"] = "",
            ["indent"] = 0,
            ["version"] = 1,
            ["children"] = new JArray(),
            ["direction"] = "ltr"
        };
        
        var text = HttpUtility.HtmlDecode(node.InnerText);
        (code["children"] as JArray)?.Add(CreateTextNode(text, 0));
        
        return code;
    }
    
    private int GetFormatFromParents(HtmlNode node)
    {
        int format = 0;
        var current = node.ParentNode;
        
        while (current != null && current.Name != "#document")
        {
            switch (current.Name.ToLower())
            {
                case "strong":
                case "b":
                    format |= 1; // Bold
                    break;
                case "i":
                case "em":
                    format |= 2; // Italic
                    break;
                case "s":
                case "strike":
                    format |= 4; // Strikethrough
                    break;
                case "u":
                    format |= 8; // Underline
                    break;
                case "code":
                    format |= 16; // Code
                    break;
            }
            current = current.ParentNode;
        }
        
        return format;
    }
}
