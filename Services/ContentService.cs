using Microsoft.EntityFrameworkCore;
using CursilloWeb.Data;
using System.Text;
using System.Web;

namespace CursilloWeb.Services;

public class ContentService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<string> GetContentAsync(string section)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Use raw SQL to get the data as binary and convert it properly
            var result = await context.Database
                .SqlQuery<BinaryContentData>($@"
                    SELECT 
                        CAST(HtmlContent AS varbinary(max)) as ContentBinary,
                        CASE WHEN HtmlContent IS NULL THEN 1 ELSE 0 END as IsNull
                    FROM ContentBlocks 
                    WHERE Section = {section}")
                .FirstOrDefaultAsync();

            if (result == null || result.IsNull == 1)
                return string.Empty;

            // Convert binary data to string
            var content = ConvertBinaryToString(result.ContentBinary) ?? string.Empty;

            // Validate and clean the content before returning
            return ValidateAndCleanContent(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetContentAsync: {ex.Message}");

            // Fallback: return safe default content
            return GetSafeDefaultContent(section);
        }
    }

    public async Task<string> GetRichTextContentAsync(string section)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Use raw SQL to get the data as binary and convert it properly
            var result = await context.Database
                .SqlQuery<BinaryContentData>($@"
                    SELECT 
                        CAST(RichTextContent AS varbinary(max)) as ContentBinary,
                        CASE WHEN RichTextContent IS NULL THEN 1 ELSE 0 END as IsNull
                    FROM ContentBlocks 
                    WHERE Section = {section}")
                .FirstOrDefaultAsync();

            if (result == null || result.IsNull == 1)
                return CreateBasicRichTextStructure(""); // Return empty rich text structure

            // Convert binary data to string
            var content = ConvertBinaryToString(result.ContentBinary) ?? string.Empty;

            // Validate rich text content
            return ValidateAndCleanRichTextContent(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRichTextContentAsync: {ex.Message}");

            // Fallback: return safe empty rich text structure
            return CreateBasicRichTextStructure("");
        }
    }

    public async Task<ContentBlock?> GetContentBlockAsync(string section)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Get the basic info and binary content separately
            var result = await context.Database
                .SqlQuery<FullBinaryContentData>($@"
                    SELECT 
                        Id,
                        Section,
                        CAST(HtmlContent AS varbinary(max)) as HtmlContentBinary,
                        CAST(RichTextContent AS varbinary(max)) as RichTextContentBinary,
                        CASE WHEN HtmlContent IS NULL THEN 1 ELSE 0 END as HtmlIsNull,
                        CASE WHEN RichTextContent IS NULL THEN 1 ELSE 0 END as RichTextIsNull,
                        LastUpdated
                    FROM ContentBlocks 
                    WHERE Section = {section}")
                .FirstOrDefaultAsync();

            if (result == null)
                return null;

            return new ContentBlock
            {
                Id = result.Id,
                Section = result.Section,
                HtmlContent = result.HtmlIsNull == 1 ? null : ConvertBinaryToString(result.HtmlContentBinary),
                RichTextContent = result.RichTextIsNull == 1 ? null : ConvertBinaryToString(result.RichTextContentBinary),
                LastUpdated = result.LastUpdated
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetContentBlockAsync: {ex.Message}");

            // Fallback: try the regular EF approach
            try
            {
                return await context.ContentBlocks.FirstOrDefaultAsync(b => b.Section == section);
            }
            catch
            {
                return null;
            }
        }
    }

    public async Task UpdateContentAsync(string section, string html)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Use raw SQL to avoid EF mapping issues entirely
            var rowsAffected = await context.Database.ExecuteSqlAsync($@"
                UPDATE ContentBlocks 
                SET HtmlContent = {html}, LastUpdated = {DateTime.Now}
                WHERE Section = {section}");

            // If no rows were updated, insert a new record
            if (rowsAffected == 0)
            {
                await context.Database.ExecuteSqlAsync($@"
                    INSERT INTO ContentBlocks (Id, Section, HtmlContent, LastUpdated)
                    VALUES ({Guid.NewGuid()}, {section}, {html}, {DateTime.Now})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateContentAsync: {ex.Message}");
            throw; // Re-throw the exception since we can't safely fall back
        }
    }

    public async Task UpdateRichTextContentAsync(string section, string richTextJson)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Use raw SQL to avoid EF mapping issues entirely
            var rowsAffected = await context.Database.ExecuteSqlAsync($@"
                UPDATE ContentBlocks 
                SET RichTextContent = {richTextJson}, LastUpdated = {DateTime.Now}
                WHERE Section = {section}");

            // If no rows were updated, insert a new record
            if (rowsAffected == 0)
            {
                await context.Database.ExecuteSqlAsync($@"
                    INSERT INTO ContentBlocks (Id, Section, RichTextContent, LastUpdated)
                    VALUES ({Guid.NewGuid()}, {section}, {richTextJson}, {DateTime.Now})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateRichTextContentAsync: {ex.Message}");
            throw; // Re-throw the exception since we can't safely fall back
        }
    }

    public async Task UpdateBothContentAsync(string section, string html, string richTextJson)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Use raw SQL to avoid EF mapping issues entirely
            var rowsAffected = await context.Database.ExecuteSqlAsync($@"
                UPDATE ContentBlocks 
                SET HtmlContent = {html}, RichTextContent = {richTextJson}, LastUpdated = {DateTime.Now}
                WHERE Section = {section}");

            // If no rows were updated, insert a new record
            if (rowsAffected == 0)
            {
                await context.Database.ExecuteSqlAsync($@"
                    INSERT INTO ContentBlocks (Id, Section, HtmlContent, RichTextContent, LastUpdated)
                    VALUES ({Guid.NewGuid()}, {section}, {html}, {richTextJson}, {DateTime.Now})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateBothContentAsync: {ex.Message}");
            throw; // Re-throw the exception since we can't safely fall back
        }
    }

    /// <summary>
    /// Fixes binary data issues by converting all content to proper strings
    /// </summary>
    public async Task FixBinaryDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            Console.WriteLine("Starting binary data fix...");

            // Create backup first
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ContentBlocks_AutoBackup]') AND type in (N'U'))
                BEGIN
                    SELECT * INTO ContentBlocks_AutoBackup FROM ContentBlocks;
                    PRINT 'Auto backup created';
                END");

            // Get all records with binary content
            var binaryRecords = await context.Database
                .SqlQuery<FullBinaryContentData>($@"
                    SELECT 
                        Id,
                        Section,
                        CAST(HtmlContent AS varbinary(max)) as HtmlContentBinary,
                        CAST(RichTextContent AS varbinary(max)) as RichTextContentBinary,
                        CASE WHEN HtmlContent IS NULL THEN 1 ELSE 0 END as HtmlIsNull,
                        CASE WHEN RichTextContent IS NULL THEN 1 ELSE 0 END as RichTextIsNull,
                        LastUpdated
                    FROM ContentBlocks")
                .ToListAsync();

            foreach (var record in binaryRecords)
            {
                var htmlText = record.HtmlIsNull == 1 ? null : ConvertBinaryToString(record.HtmlContentBinary);
                var richTextText = record.RichTextIsNull == 1 ? null : ConvertBinaryToString(record.RichTextContentBinary);

                // Update with converted text
                await context.Database.ExecuteSqlAsync($@"
                    UPDATE ContentBlocks 
                    SET 
                        HtmlContent = {htmlText},
                        RichTextContent = {richTextText},
                        LastUpdated = {DateTime.Now}
                    WHERE Id = {record.Id}");
            }

            Console.WriteLine($"Fixed {binaryRecords.Count} ContentBlock records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in FixBinaryDataAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Converts HTML content to RichText format for all ContentBlocks that have HTML but no RichText
    /// </summary>
    public async Task ConvertHtmlToRichTextAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Use raw SQL to get HTML content safely
            var blocksToConvert = await context.Database
                .SqlQuery<ConversionData>($@"
                    SELECT 
                        Id, 
                        Section, 
                        CONVERT(nvarchar(max), HtmlContent) as HtmlContent
                    FROM ContentBlocks 
                    WHERE HtmlContent IS NOT NULL 
                    AND LEN(CONVERT(nvarchar(max), HtmlContent)) > 0 
                    AND (RichTextContent IS NULL OR LEN(CONVERT(nvarchar(max), RichTextContent)) = 0)")
                .ToListAsync();

            foreach (var block in blocksToConvert)
            {
                try
                {
                    var richTextJson = ConvertHtmlToRichText(block.HtmlContent);

                    await context.Database.ExecuteSqlAsync($@"
                        UPDATE ContentBlocks 
                        SET RichTextContent = {richTextJson}, LastUpdated = {DateTime.Now}
                        WHERE Id = {block.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting HTML to RichText for section '{block.Section}': {ex.Message}");

                    var fallbackRichText = CreateBasicRichTextStructure(block.HtmlContent ?? string.Empty);
                    await context.Database.ExecuteSqlAsync($@"
                        UPDATE ContentBlocks 
                        SET RichTextContent = {fallbackRichText}, LastUpdated = {DateTime.Now}
                        WHERE Id = {block.Id}");
                }
            }

            Console.WriteLine($"Converted {blocksToConvert.Count} blocks from HTML to RichText");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ConvertHtmlToRichTextAsync: {ex.Message}");
            throw;
        }
    }

    private static string? ConvertBinaryToString(byte[]? binaryData)
    {
        if (binaryData == null || binaryData.Length == 0)
            return null;

        try
        {
            // Try UTF-8 first
            var text = Encoding.UTF8.GetString(binaryData);

            // Check if the conversion was successful (no replacement characters)
            if (!text.Contains('\uFFFD'))
                return text;

            // Fallback to UTF-16 (Unicode)
            if (binaryData.Length % 2 == 0)
            {
                text = Encoding.Unicode.GetString(binaryData);
                if (!text.Contains('\uFFFD'))
                    return text;
            }

            // Last resort: ASCII
            return Encoding.ASCII.GetString(binaryData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting binary to string: {ex.Message}");
            return Encoding.ASCII.GetString(binaryData);
        }
    }

    /// <summary>
    /// Validates and cleans HTML content to prevent corruption issues
    /// </summary>
    private static string ValidateAndCleanContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        try
        {
            // Remove any null characters that might cause issues
            content = content.Replace('\0', ' ').Trim();

            // Check for obvious binary corruption indicators
            if (content.Length > 0 && content.All(c => c == '\0' || char.IsControl(c) && c != '\r' && c != '\n' && c != '\t'))
            {
                Console.WriteLine("Detected corrupted content, returning safe default");
                return "<p>Content temporarily unavailable. Please refresh the page.</p>";
            }

            // Basic HTML validation - ensure content looks like HTML
            if (content.Contains('<') && content.Contains('>'))
            {
                return content; // Looks like valid HTML
            }

            // If it doesn't look like HTML, wrap it in a paragraph
            if (!string.IsNullOrWhiteSpace(content))
            {
                return $"<p>{System.Web.HttpUtility.HtmlEncode(content)}</p>";
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating content: {ex.Message}");
            return "<p>Content validation failed. Please contact administrator.</p>";
        }
    }

    /// <summary>
    /// Validates and cleans rich text JSON content
    /// </summary>
    private static string ValidateAndCleanRichTextContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return CreateBasicRichTextStructure("");

        try
        {
            // Remove any null characters
            content = content.Replace('\0', ' ').Trim();

            // Check if it looks like JSON
            if (content.StartsWith('{') && content.EndsWith('}'))
            {
                // Try to parse as JSON to validate
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(content);
                    return content; // Valid JSON
                }
                catch
                {
                    Console.WriteLine("Invalid JSON detected in rich text content, creating basic structure");
                    return CreateBasicRichTextStructure("Content format error - please re-enter content.");
                }
            }

            // If it's not JSON, treat it as plain text
            return CreateBasicRichTextStructure(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating rich text content: {ex.Message}");
            return CreateBasicRichTextStructure("Content validation failed - please contact administrator.");
        }
    }

    /// <summary>
    /// Returns safe default content for a given section
    /// </summary>
    private static string GetSafeDefaultContent(string section)
    {
        return section.ToLower() switch
        {
            "header" => "<h1>Welcome</h1><p>Header content will be displayed here.</p>",
            "footer" => "<p>&copy; 2025 Cursillo Web. All rights reserved.</p>",
            _ => "<p>Content will be displayed here once configured.</p>"
        };
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

// Helper classes for raw SQL queries to handle binary data conversion
public class BinaryContentData
{
    public byte[]? ContentBinary { get; set; }
    public int IsNull { get; set; }
}

public class FullBinaryContentData
{
    public Guid Id { get; set; }
    public string Section { get; set; } = string.Empty;
    public byte[]? HtmlContentBinary { get; set; }
    public byte[]? RichTextContentBinary { get; set; }
    public int HtmlIsNull { get; set; }
    public int RichTextIsNull { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ConversionData
{
    public Guid Id { get; set; }
    public string Section { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
}
