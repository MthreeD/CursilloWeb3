using Microsoft.EntityFrameworkCore;
using CursilloWeb.Data;
using System.Text;
using System.Web;
using System.Linq;

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

    public async Task<string> GetRTFContentAsync(string section)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Use raw SQL to get the data as binary and convert it properly
            var result = await context.Database
                .SqlQuery<BinaryContentData>($@"
                    SELECT 
                        CAST(RTFContent AS varbinary(max)) as ContentBinary,
                        CASE WHEN RTFContent IS NULL THEN 1 ELSE 0 END as IsNull
                    FROM ContentBlocks 
                    WHERE Section = {section}")
                .FirstOrDefaultAsync();

            if (result == null || result.IsNull == 1)
                return GetSafeDefaultRTFContent(section); // Return default RTF content

            // Convert binary data to string (RTF content)
            var content = ConvertBinaryToString(result.ContentBinary) ?? string.Empty;

            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRTFContentAsync: {ex.Message}");

            // Fallback: return safe default RTF content
            return GetSafeDefaultRTFContent(section);
        }
    }

    /// <summary>
    /// Gets RTF content as byte array for DxRichEdit DocumentContent binding
    /// </summary>
    public async Task<byte[]> GetRTFDocumentContentAsync(string section)
    {
        var rtfString = await GetRTFContentAsync(section);
        if (string.IsNullOrEmpty(rtfString))
        {
            return GetDefaultRTFBytes(section);
        }

        return System.Text.Encoding.UTF8.GetBytes(rtfString);
    }

    /// <summary>
    /// Updates RTF content from byte array (from DxRichEdit DocumentContent)
    /// </summary>
    public async Task UpdateRTFDocumentContentAsync(string section, byte[] rtfDocumentContent)
    {
        Console.WriteLine($"UpdateRTFDocumentContentAsync called for section: {section}");
        Console.WriteLine($"RTF document content length: {rtfDocumentContent?.Length ?? 0}");

        if (rtfDocumentContent == null || rtfDocumentContent.Length == 0)
        {
            Console.WriteLine("Warning: Empty RTF document content received");
            return;
        }

        var rtfString = System.Text.Encoding.UTF8.GetString(rtfDocumentContent);
        Console.WriteLine($"RTF string length: {rtfString.Length}");
        Console.WriteLine($"RTF content preview: {rtfString.Substring(0, Math.Min(100, rtfString.Length))}...");

        await UpdateRTFContentAsync(section, rtfString);
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
                        CAST(RTFContent AS varbinary(max)) as RichTextContentBinary,
                        CASE WHEN HtmlContent IS NULL THEN 1 ELSE 0 END as HtmlIsNull,
                        CASE WHEN RTFContent IS NULL THEN 1 ELSE 0 END as RichTextIsNull,
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
                RTFContent = result.RichTextIsNull == 1 ? null : ConvertBinaryToString(result.RichTextContentBinary),
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
            Console.WriteLine($"UpdateContentAsync called for section: {section}");
            Console.WriteLine($"Content length: {html?.Length ?? 0}");

            // First, let's test if we can select from the table
            var existingBlock = await context.ContentBlocks
                .Where(b => b.Section == section)
                .FirstOrDefaultAsync();

            Console.WriteLine($"Existing block found: {existingBlock != null}");

            // Use raw SQL to avoid EF mapping issues entirely
            var rowsAffected = await context.Database.ExecuteSqlAsync($@"
                UPDATE ContentBlocks 
                SET HtmlContent = {html}, LastUpdated = {DateTime.Now}
                WHERE Section = {section}");

            Console.WriteLine($"Rows affected by update: {rowsAffected}");

            // If no rows were updated, insert a new record
            if (rowsAffected == 0)
            {
                Console.WriteLine("No rows updated, inserting new record...");
                await context.Database.ExecuteSqlAsync($@"
                    INSERT INTO ContentBlocks (Id, Section, HtmlContent, LastUpdated)
                    VALUES ({Guid.NewGuid()}, {section}, {html}, {DateTime.Now})");
                Console.WriteLine("Insert completed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateContentAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw; // Re-throw the exception since we can't safely fall back
        }
    }

    public async Task UpdateRTFContentAsync(string section, string richTextJson)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            Console.WriteLine($"UpdateRTFContentAsync called for section: {section}");
            Console.WriteLine($"RTF content length: {richTextJson?.Length ?? 0}");

            // Use raw SQL to avoid EF mapping issues entirely
            var rowsAffected = await context.Database.ExecuteSqlAsync($@"
                UPDATE ContentBlocks 
                SET RTFContent = {richTextJson}, LastUpdated = {DateTime.Now}
                WHERE Section = {section}");

            Console.WriteLine($"Rows affected by RTF update: {rowsAffected}");

            // If no rows were updated, insert a new record
            if (rowsAffected == 0)
            {
                Console.WriteLine("No rows updated, inserting new RTF record...");
                await context.Database.ExecuteSqlAsync($@"
                    INSERT INTO ContentBlocks (Id, Section, RTFContent, LastUpdated)
                    VALUES ({Guid.NewGuid()}, {section}, {richTextJson}, {DateTime.Now})");
                Console.WriteLine("RTF insert completed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateRTFContentAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                SET HtmlContent = {html}, RTFContent = {richTextJson}, LastUpdated = {DateTime.Now}
                WHERE Section = {section}");

            // If no rows were updated, insert a new record
            if (rowsAffected == 0)
            {
                await context.Database.ExecuteSqlAsync($@"
                    INSERT INTO ContentBlocks (Id, Section, HtmlContent, RTFContent, LastUpdated)
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
                        CAST(RTFContent AS varbinary(max)) as RichTextContentBinary,
                        CASE WHEN HtmlContent IS NULL THEN 1 ELSE 0 END as HtmlIsNull,
                        CASE WHEN RTFContent IS NULL THEN 1 ELSE 0 END as RichTextIsNull,
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
                        RTFContent = {richTextText},
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
            // Use raw SQL to get HTML content safely for blocks that need conversion
            var blocksToConvert = await context.Database
                .SqlQuery<ConversionData>($@"
                    SELECT 
                        Id, 
                        Section, 
                        CONVERT(nvarchar(max), HtmlContent) as HtmlContent
                    FROM ContentBlocks 
                    WHERE HtmlContent IS NOT NULL 
                    AND LEN(CONVERT(nvarchar(max), HtmlContent)) > 0 
                    AND (RTFContent IS NULL OR LEN(CONVERT(nvarchar(max), RTFContent)) = 0)")
                .ToListAsync();

            foreach (var block in blocksToConvert)
            {
                try
                {
                    var richTextJson = ConvertHtmlToRichText(block.HtmlContent ?? string.Empty);

                    await context.Database.ExecuteSqlAsync($@"
                        UPDATE ContentBlocks 
                        SET RTFContent = {richTextJson}, LastUpdated = {DateTime.Now}
                        WHERE Id = {block.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting HTML to RichText for section '{block.Section}': {ex.Message}");

                    var fallbackRichText = ConvertHtmlToRichText(block.HtmlContent ?? string.Empty);
                    await context.Database.ExecuteSqlAsync($@"
                        UPDATE ContentBlocks 
                        SET RTFContent = {fallbackRichText}, LastUpdated = {DateTime.Now}
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
            return GetSafeDefaultRTFContent("default");

        try
        {
            // Remove any null characters
            content = content.Replace('\0', ' ').Trim();

            // For RTF content, just return as-is if it looks like RTF
            if (content.StartsWith(@"{\rtf"))
            {
                return content; // Valid RTF
            }

            // If it's not RTF, convert to RTF
            return ConvertHtmlToRichText(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating rich text content: {ex.Message}");
            return GetSafeDefaultRTFContent("default");
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

    /// <summary>
    /// Returns safe default RTF content for a given section
    /// </summary>
    private static string GetSafeDefaultRTFContent(string section)
    {
        var defaultText = section.ToLower() switch
        {
            "header" => "Welcome - Header content will be displayed here.",
            "footer" => "© 2025 Cursillo Web. All rights reserved.",
            _ => "Content will be displayed here once configured."
        };

        // Return actual RTF format, not JSON
        return $@"{{\rtf1\ansi\deff0 {{\fonttbl {{\f0 Times New Roman;}}}}{{\colortbl;\red0\green0\blue0;}}\f0\fs24 {defaultText}}}";
    }

    /// <summary>
    /// Gets default RTF content as byte array
    /// </summary>
    private static byte[] GetDefaultRTFBytes(string section)
    {
        var rtfString = GetSafeDefaultRTFContent(section);
        return System.Text.Encoding.UTF8.GetBytes(rtfString);
    }

    private static string ConvertHtmlToRichText(string htmlContent)
    {
        // Convert HTML to RTF format (simple conversion)
        var plainText = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<[^>]*>", "").Trim();
        return $@"{{\rtf1\ansi\deff0 {{\fonttbl {{\f0 Times New Roman;}}}}{{\colortbl;\red0\green0\blue0;}}\f0\fs24 {plainText}}}";
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
