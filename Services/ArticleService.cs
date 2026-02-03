using Microsoft.EntityFrameworkCore;
using CursilloWeb.Data;

namespace CursilloWeb.Services;

public class ArticleService(ApplicationDbContext context)
{
    public async Task<List<Article>> GetArticlesAsync(bool onlyVisible = true)
    {
        IQueryable<Article> query = context.Articles;
        if (onlyVisible)
        {
            query = query.Where(a => a.IsVisible);
        }
        return await query.OrderBy(a => a.Order).ThenByDescending(a => a.CreatedDate).ToListAsync();
    }

    public async Task<Article?> GetArticleByIdAsync(int id)
    {
        return await context.Articles.FindAsync(id);
    }

    public async Task CreateArticleAsync(Article article)
    {
        context.Articles.Add(article);
        await context.SaveChangesAsync();
    }

    public async Task UpdateArticleAsync(Article article)
    {
        context.Articles.Update(article);
        await context.SaveChangesAsync();
    }

    public async Task DeleteArticleAsync(int id)
    {
        var article = await context.Articles.FindAsync(id);
        if (article != null)
        {
            context.Articles.Remove(article);
            await context.SaveChangesAsync();
        }
    }
}
