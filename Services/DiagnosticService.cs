using Microsoft.EntityFrameworkCore;
using CursilloWeb.Data;

namespace CursilloWeb.Services;

public class DiagnosticService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<string> DiagnoseContentBlockIssueAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        
        try
        {
            // Try to get raw data without accessing specific properties
            var blocks = await context.ContentBlocks.Select(cb => new 
            { 
                cb.Id, 
                cb.Section,
                cb.LastUpdated
            }).ToListAsync();
            
            return $"Found {blocks.Count} ContentBlocks without accessing content properties";
        }
        catch (Exception ex)
        {
            return $"Error reading basic properties: {ex.Message}";
        }
    }

    public async Task<string> DiagnoseHtmlContentAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        
        try
        {
            // Try to access just the HtmlContent property
            var firstBlock = await context.ContentBlocks
                .Select(cb => new { cb.Id, cb.Section, cb.HtmlContent })
                .FirstOrDefaultAsync();
                
            return firstBlock?.HtmlContent == null 
                ? "HtmlContent is null" 
                : $"HtmlContent length: {firstBlock.HtmlContent.Length}";
        }
        catch (Exception ex)
        {
            return $"Error reading HtmlContent: {ex.Message} - {ex.GetType().Name}";
        }
    }

    public async Task<string> DiagnoseRTFContentAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        try
        {
            // Try to access just the RTFContent property
            var firstBlock = await context.ContentBlocks
                .Select(cb => new { cb.Id, cb.Section, cb.RTFContent })
                .FirstOrDefaultAsync();

            return firstBlock?.RTFContent == null 
                ? "RTFContent is null" 
                : $"RTFContent length: {firstBlock.RTFContent.Length}";
        }
        catch (Exception ex)
        {
            return $"Error reading RTFContent: {ex.Message} - {ex.GetType().Name}";
        }
    }
}