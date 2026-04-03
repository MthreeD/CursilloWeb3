using CursilloWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CursilloWeb.Services;

/// <summary>
/// Service to fix the InvalidCastException by handling binary data properly
/// </summary>
public class DatabaseFixService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    /// <summary>
    /// Call this once to fix the binary data issue in ContentBlocks table
    /// </summary>
    public async Task FixContentBlocksDataAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            Console.WriteLine("Starting ContentBlocks database fix...");

            // Step 1: Create a backup table
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ContentBlocks_Backup]') AND type in (N'U'))
                BEGIN
                    SELECT * INTO ContentBlocks_Backup FROM ContentBlocks;
                    PRINT 'Backup created successfully';
                END");

            // Step 2: Read all data safely
            var records = await context.Database
                .SqlQuery<ContentBlockRaw>($@"
                    SELECT 
                        Id,
                        Section,
                        CONVERT(nvarchar(max), HtmlContent) as HtmlContentText,
                        CONVERT(nvarchar(max), RichTextContent) as RichTextContentText,
                        LastUpdated
                    FROM ContentBlocks")
                .ToListAsync();

            Console.WriteLine($"Found {records.Count} ContentBlocks to fix");

            // Step 3: Drop and recreate table with clean schema
            await context.Database.ExecuteSqlRawAsync("DROP TABLE ContentBlocks;");

            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE ContentBlocks (
                    Id uniqueidentifier NOT NULL PRIMARY KEY DEFAULT NEWID(),
                    Section nvarchar(max) NOT NULL,
                    HtmlContent nvarchar(max) NULL,
                    RichTextContent nvarchar(max) NULL,
                    LastUpdated datetime2 NOT NULL DEFAULT GETDATE()
                );");

            // Step 4: Insert cleaned data
            foreach (var record in records)
            {
                await context.Database.ExecuteSqlAsync($@"
                    INSERT INTO ContentBlocks (Id, Section, HtmlContent, RichTextContent, LastUpdated)
                    VALUES ({record.Id}, {record.Section}, {record.HtmlContentText}, {record.RichTextContentText}, {record.LastUpdated})");
            }

            Console.WriteLine("ContentBlocks data fix completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fixing ContentBlocks data: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Diagnose the current state of the ContentBlocks table
    /// </summary>
    public async Task<string> DiagnoseContentBlocksAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Check table structure
            var columns = await context.Database
                .SqlQuery<ColumnInfo>($@"
                    SELECT 
                        COLUMN_NAME as Name,
                        DATA_TYPE as Type,
                        IS_NULLABLE as IsNullable
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'ContentBlocks'
                    ORDER BY ORDINAL_POSITION")
                .ToListAsync();

            var columnInfo = string.Join("\n", columns.Select(c => $"  {c.Name}: {c.Type} ({(c.IsNullable == "YES" ? "nullable" : "not null")})"));

            // Check data
            var recordCount = await context.Database
                .SqlQuery<int>($"SELECT COUNT(*) as Value FROM ContentBlocks")
                .FirstOrDefaultAsync();

            return $@"ContentBlocks Table Diagnosis:

Table Structure:
{columnInfo}

Record Count: {recordCount}

Status: Ready for diagnosis";
        }
        catch (Exception ex)
        {
            return $"Error during diagnosis: {ex.Message}";
        }
    }
}

public class ContentBlockRaw
{
    public Guid Id { get; set; }
    public string Section { get; set; } = string.Empty;
    public string? HtmlContentText { get; set; }
    public string? RichTextContentText { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string IsNullable { get; set; } = string.Empty;
}