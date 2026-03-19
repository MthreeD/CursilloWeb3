using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;

namespace CursilloWeb.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Article> Articles { get; set; }
    public DbSet<ContentBlock> ContentBlocks { get; set; }

    // XAF Security System Tables
    public DbSet<PermissionPolicyUser> PermissionPolicyUsers { get; set; }
    public DbSet<PermissionPolicyRole> PermissionPolicyRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Let XAF initialize its own security system types mapping
        builder.Entity<PermissionPolicyUser>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("PermissionPolicyUserRoles"));
    }
}

