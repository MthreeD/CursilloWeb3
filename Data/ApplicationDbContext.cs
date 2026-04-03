using Microsoft.EntityFrameworkCore;

namespace CursilloWeb.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :DbContext(options)
{
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<ContentBlock> ContentBlocks { get; set; } = null!;
    public DbSet<Test> Tests { get; set; } = null!;
    public DbSet<WebmasterSetting> WebmasterSettings { get; set; } = null!;
    public DbSet<NewFooterContent> NewFooterContents { get; set; } = null!;
    public DbSet<ApplicationSettings> ApplicationSettings { get; set; } = null!;
}
