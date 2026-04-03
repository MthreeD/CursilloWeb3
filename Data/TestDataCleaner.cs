using Microsoft.EntityFrameworkCore;

namespace CursilloWeb.Data;

/// <summary>
/// Utility class to clean up corrupted Test data in the database
/// </summary>
public static class TestDataCleaner
{
    /// <summary>
    /// Cleans up corrupted Test records by resetting them to valid RTF format
    /// </summary>
    public static async Task CleanCorruptedTestDataAsync(ApplicationDbContext context)
    {
        try
        {
            var tests = await context.Tests.ToListAsync();

            foreach (var test in tests)
            {
                bool needsUpdate = false;

                // Check if RTFContent is null or empty
                if (test.RTFContent == null || test.RTFContent.Length == 0)
                {
                    test.RTFContent = CreateDefaultRtfContent();
                    needsUpdate = true;
                }
                else
                {
                    // Validate the content is valid RTF
                    try
                    {
                        string content = System.Text.Encoding.UTF8.GetString(test.RTFContent);
                        // Check if it starts with RTF header
                        if (!content.StartsWith("{\\rtf"))
                        {
                            // Content is not RTF, reset to default
                            test.RTFContent = CreateDefaultRtfContent();
                            needsUpdate = true;
                        }
                    }
                    catch
                    {
                        // Failed to read content, reset to default
                        test.RTFContent = CreateDefaultRtfContent();
                        needsUpdate = true;
                    }
                }

                // Set default values for other fields if needed
                if (string.IsNullOrEmpty(test.FontName))
                {
                    test.FontName = "Segoe UI Light";
                    needsUpdate = true;
                }

                if (string.IsNullOrEmpty(test.FontSize))
                {
                    test.FontSize = "12pt";
                    needsUpdate = true;
                }

                if (string.IsNullOrEmpty(test.BackgroundColor))
                {
                    test.BackgroundColor = "#66CDFF";
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    test.LastUpdated = DateTime.Now;
                    context.Tests.Update(test);
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to clean corrupted test data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a default RTF document with standard formatting
    /// </summary>
    private static byte[] CreateDefaultRtfContent()
    {
        string rtfContent = @"{\rtf1\ansi\deff0 {\fonttbl {\f0 Segoe UI Light;}}
\f0\fs24 Type your content here...}";
        return System.Text.Encoding.UTF8.GetBytes(rtfContent);
    }

    /// <summary>
    /// Deletes all Test records (use with caution!)
    /// </summary>
    public static async Task DeleteAllTestsAsync(ApplicationDbContext context)
    {
        var tests = await context.Tests.ToListAsync();
        context.Tests.RemoveRange(tests);
        await context.SaveChangesAsync();
    }
}
