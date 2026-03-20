using Microsoft.EntityFrameworkCore;
using CursilloWeb.Data;

namespace CursilloWeb.Services;

public class ContentService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<string> GetContentAsync(string section)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var block = await context.ContentBlocks.FirstOrDefaultAsync(b => b.Section == section);
        return block?.HtmlContent ?? string.Empty;
    }

    public async Task<string> GetRichTextContentAsync(string section)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var block = await context.ContentBlocks.FirstOrDefaultAsync(b => b.Section == section);
        return block?.RichTextContent ?? string.Empty;
    }

    public async Task<ContentBlock?> GetContentBlockAsync(string section)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.ContentBlocks.FirstOrDefaultAsync(b => b.Section == section);
    }

    public async Task UpdateContentAsync(string section, string html)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var block = await context.ContentBlocks.FirstOrDefaultAsync(b => b.Section == section);
        if (block == null)
        {
            block = new ContentBlock { Section = section, HtmlContent = html };
            context.ContentBlocks.Add(block);
        }
        else
        {
            block.HtmlContent = html;
            block.LastUpdated = DateTime.Now;
            context.ContentBlocks.Update(block);
        }
        await context.SaveChangesAsync();
    }

    public async Task UpdateRichTextContentAsync(string section, string richTextJson)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var block = await context.ContentBlocks.FirstOrDefaultAsync(b => b.Section == section);
        if (block == null)
        {
            block = new ContentBlock { Section = section, RichTextContent = richTextJson };
            context.ContentBlocks.Add(block);
        }
        else
        {
            block.RichTextContent = richTextJson;
            block.LastUpdated = DateTime.Now;
            context.ContentBlocks.Update(block);
        }
        await context.SaveChangesAsync();
    }

    public async Task UpdateBothContentAsync(string section, string html, string richTextJson)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var block = await context.ContentBlocks.FirstOrDefaultAsync(b => b.Section == section);
        if (block == null)
        {
            block = new ContentBlock 
            { 
                Section = section, 
                HtmlContent = html, 
                RichTextContent = richTextJson 
            };
            context.ContentBlocks.Add(block);
        }
        else
        {
            block.HtmlContent = html;
            block.RichTextContent = richTextJson;
            block.LastUpdated = DateTime.Now;
            context.ContentBlocks.Update(block);
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Converts HTML content to RichText format for all ContentBlocks that have HTML but no RichText
    /// </summary>
    public async Task ConvertHtmlToRichTextAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var blocksToConvert = await context.ContentBlocks
            .Where(cb => !string.IsNullOrEmpty(cb.HtmlContent) && 
                        (cb.RichTextContent == null || cb.RichTextContent == string.Empty))
            .ToListAsync();

        foreach (var block in blocksToConvert)
        {
            try
            {
                var richTextJson = ConvertHtmlToRichText(block.HtmlContent!);
                block.RichTextContent = richTextJson;
                block.LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Log the error and set a basic rich text structure
                Console.WriteLine($"Error converting HTML to RichText for section '{block.Section}': {ex.Message}");
                block.RichTextContent = CreateBasicRichTextStructure(block.HtmlContent ?? string.Empty);
            }
        }

        if (blocksToConvert.Count > 0)
        {
            await context.SaveChangesAsync();
        }
    }

    private static string ConvertHtmlToRichText(string htmlContent)
    {
        // Simple conversion - in a real application you might use a library like AngleSharp or HtmlAgilityPack
        var plainText = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<[^>]*>", "").Trim();

        return CreateBasicRichTextStructure(plainText);
    }

    private static string CreateBasicRichTextStructure(string text)
    {
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
                        new { type = "text", text = text }
                    }
                }
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(richTextDocument, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = false,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }
}
