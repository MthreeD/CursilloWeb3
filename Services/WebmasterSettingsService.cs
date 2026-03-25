using Microsoft.EntityFrameworkCore;
using CursilloWeb.Data;

namespace CursilloWeb.Services;

public class WebmasterSettingsService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<List<WebmasterSetting>> GetAllSettingsAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.WebmasterSettings
            .OrderBy(w => w.Description)
            .ToListAsync();
    }

    public async Task<WebmasterSetting?> GetSettingByIdAsync(Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.WebmasterSettings.FindAsync(id);
    }

    public async Task CreateSettingAsync(WebmasterSetting setting)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        setting.Id = Guid.NewGuid();
        setting.CreatedDate = DateTime.Now;
        context.WebmasterSettings.Add(setting);
        await context.SaveChangesAsync();
    }

    public async Task UpdateSettingAsync(WebmasterSetting setting)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        setting.ModifiedDate = DateTime.Now;
        context.WebmasterSettings.Update(setting);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSettingAsync(Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var setting = await context.WebmasterSettings.FindAsync(id);
        if (setting != null)
        {
            context.WebmasterSettings.Remove(setting);
            await context.SaveChangesAsync();
        }
    }

    public async Task SaveAllSettingsAsync(List<WebmasterSetting> settings)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        
        foreach (var setting in settings)
        {
            if (setting.Id == Guid.Empty)
            {
                // New setting
                setting.Id = Guid.NewGuid();
                setting.CreatedDate = DateTime.Now;
                context.WebmasterSettings.Add(setting);
            }
            else
            {
                // Existing setting
                setting.ModifiedDate = DateTime.Now;
                context.WebmasterSettings.Update(setting);
            }
        }
        
        await context.SaveChangesAsync();
    }
}