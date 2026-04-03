using CursilloWeb.Data;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CursilloWeb.DataMigration;

public class HtmlToRichTextConverter(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task ConvertAllHtmlToRichTextAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var contentBlocks = await context.ContentBlocks
            .Where(cb => !string.IsNullOrEmpty(cb.HtmlContent) && string.IsNullOrEmpty(cb.RTFContent))
            .ToListAsync();

        foreach (var block in contentBlocks)
        {
            try
            {
                var richTextJson = ConvertHtmlToRichText(block.HtmlContent);
                block.RTFContent = richTextJson;
                block.LastUpdated = DateTime.Now;
                Console.WriteLine($"Converted content for section: {block.Section}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting content for section {block.Section}: {ex.Message}");
                // Set a basic rich text structure for failed conversions
                block.RTFContent = CreateBasicRichTextStructure(block.HtmlContent);
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Converted {contentBlocks.Count} content blocks to RichText format.");
    }

    private static string ConvertHtmlToRichText(string? htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return CreateEmptyRichTextStructure();
        }

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var richTextDocument = new
            {
                type = "doc",
                content = ProcessNodes(doc.DocumentNode.ChildNodes)
            };

            return JsonSerializer.Serialize(richTextDocument, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            // Fallback to basic text conversion
            return CreateBasicRichTextStructure(htmlContent);
        }
    }

    private static object[] ProcessNodes(HtmlNodeCollection nodes)
    {
        var content = new List<object>();

        foreach (var node in nodes)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Text:
                    var text = node.InnerText.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        content.Add(new
                        {
                            type = "paragraph",
                            content = new[]
                            {
                                new { type = "text", text }
                            }
                        });
                    }
                    break;

                case HtmlNodeType.Element:
                    content.Add(ProcessElement(node));
                    break;
            }
        }

        return content.ToArray();
    }

    private static object ProcessElement(HtmlNode element)
    {
        return element.Name.ToLowerInvariant() switch
        {
            "p" => new
            {
                type = "paragraph",
                content = ProcessInlineNodes(element.ChildNodes)
            },
            "h1" => new
            {
                type = "heading",
                attrs = new { level = 1 },
                content = ProcessInlineNodes(element.ChildNodes)
            },
            "h2" => new
            {
                type = "heading",
                attrs = new { level = 2 },
                content = ProcessInlineNodes(element.ChildNodes)
            },
            "h3" => new
            {
                type = "heading",
                attrs = new { level = 3 },
                content = ProcessInlineNodes(element.ChildNodes)
            },
            "h4" => new
            {
                type = "heading",
                attrs = new { level = 4 },
                content = ProcessInlineNodes(element.ChildNodes)
            },
            "h5" => new
            {
                type = "heading",
                attrs = new { level = 5 },
                content = ProcessInlineNodes(element.ChildNodes)
            },
            "h6" => new
            {
                type = "heading",
                attrs = new { level = 6 },
                content = ProcessInlineNodes(element.ChildNodes)
            },
            "ul" => new
            {
                type = "bulletList",
                content = element.ChildNodes
                    .Where(n => n.Name == "li")
                    .Select(li => new
                    {
                        type = "listItem",
                        content = new[]
                        {
                            new
                            {
                                type = "paragraph",
                                content = ProcessInlineNodes(li.ChildNodes)
                            }
                        }
                    }).ToArray()
            },
            "ol" => new
            {
                type = "orderedList",
                content = element.ChildNodes
                    .Where(n => n.Name == "li")
                    .Select(li => new
                    {
                        type = "listItem",
                        content = new[]
                        {
                            new
                            {
                                type = "paragraph",
                                content = ProcessInlineNodes(li.ChildNodes)
                            }
                        }
                    }).ToArray()
            },
            "br" => new
            {
                type = "hardBreak"
            },
            _ => new
            {
                type = "paragraph",
                content = ProcessInlineNodes(element.ChildNodes)
            }
        };
    }

    private static object[] ProcessInlineNodes(HtmlNodeCollection nodes)
    {
        var content = new List<object>();

        foreach (var node in nodes)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                var text = node.InnerText;
                if (!string.IsNullOrEmpty(text))
                {
                    content.Add(new { type = "text", text });
                }
            }
            else if (node.NodeType == HtmlNodeType.Element)
            {
                content.Add(ProcessInlineElement(node));
            }
        }

        return content.ToArray();
    }

    private static object ProcessInlineElement(HtmlNode element)
    {
        var text = element.InnerText;
        var marks = new List<object>();

        switch (element.Name.ToLowerInvariant())
        {
            case "strong" or "b":
                marks.Add(new { type = "bold" });
                break;
            case "em" or "i":
                marks.Add(new { type = "italic" });
                break;
            case "u":
                marks.Add(new { type = "underline" });
                break;
            case "a":
                var href = element.GetAttributeValue("href", string.Empty);
                marks.Add(new { type = "link", attrs = new { href } });
                break;
        }

        return marks.Count > 0
            ? new { type = "text", text, marks = marks.ToArray() }
            : new { type = "text", text };
    }

    private static string CreateEmptyRichTextStructure()
    {
        var richTextDocument = new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    content = Array.Empty<object>()
                }
            }
        };

        return JsonSerializer.Serialize(richTextDocument, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static string CreateBasicRichTextStructure(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return CreateEmptyRichTextStructure();

        // Strip HTML tags for basic fallback
        var plainText = Regex.Replace(text, "<[^>]*>", string.Empty).Trim();

        var richTextDocument = new
        {
            type = "doc",
            content = new[]
            {
                new
                {
                    type = "paragraph",
                    content = new[]
                    {
                        new { type = "text", text = plainText }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(richTextDocument, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}