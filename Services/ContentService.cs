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
}
