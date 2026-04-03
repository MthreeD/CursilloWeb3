using CursilloWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CursilloWeb.Services;

public class ArticleService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<List<Article>> GetArticlesAsync(bool onlyVisible = true)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        IQueryable<Article> query = context.Articles;
        if (onlyVisible)
        {
            query = query.Where(a => a.IsVisible);
        }
        return await query.OrderBy(a => a.Order).ThenByDescending(a => a.CreatedDate).ToListAsync();
    }

    public async Task<Article?> GetArticleByIdAsync(Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Articles.FindAsync(id);
    }

    public async Task CreateArticleAsync(Article article)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        context.Articles.Add(article);
        await context.SaveChangesAsync();
    }

    public async Task UpdateArticleAsync(Article article)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        context.Articles.Update(article);
        await context.SaveChangesAsync();
    }

    public async Task DeleteArticleAsync(Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var article = await context.Articles.FindAsync(id);
        if (article != null)
        {
            context.Articles.Remove(article);
            await context.SaveChangesAsync();
        }
    }
}
