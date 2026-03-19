using Microsoft.EntityFrameworkCore;

namespace CursilloWeb.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles { get; set; }
    public DbSet<ContentBlock> ContentBlocks { get; set; }
}
