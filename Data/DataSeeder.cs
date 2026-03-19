using CursilloWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CursilloWeb.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        if (!await context.ContentBlocks.AnyAsync())
        {
            context.ContentBlocks.AddRange(
                new ContentBlock
                {
                    Section = "Header",
                    HtmlContent = @"<div class=""d-flex align-items-center"">
                                    <div class=""me-3"">
                                        <!-- Placeholder wrapper for Logo -->
                                        <div style=""width: 80px; height: 80px; background-color: #003366; color: #D4AF37; display: flex; align-items: center; justify-content: center; border-radius: 50%; font-weight: bold; font-size: 2rem;"">
                                            C
                                        </div>
                                    </div>
                                    <div>
                                        <h1 class=""h3 mb-0"" style=""color: #003366; font-family: 'Playfair Display', serif;"">Venice Diocese Cursillo</h1>
                                        <small class=""text-muted"">Make a friend, be a friend, bring a friend to Christ</small>
                                    </div>
                                </div>",
                    LastUpdated = DateTime.Now
                },
                new ContentBlock
                {
                    Section = "Footer",
                    HtmlContent = @"<div class=""row"">
                                    <div class=""col-md-6"">
                                        <h5>Contact Us</h5>
                                        <p>Venice Diocese Cursillo<br>
                                        1000 Cursillo Way<br>
                                        Venice, FL 34285</p>
                                    </div>
                                    <div class=""col-md-6 text-md-end"">
                                        <p>&copy; 2026 Cursillo Web. All Rights Reserved.</p>
                                        <p><small>Powered by <strong>Xari Team</strong></small></p>
                                    </div>
                                </div>",
                    LastUpdated = DateTime.Now
                }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Articles.AnyAsync())
        {
            context.Articles.AddRange(
                new Article
                {
                    Title = "Welcome to the Cursillo Movement",
                    Content = @"<p>The Cursillo Movement is a movement of the Church which by its own Method attempts from within the Church, to give life to the essential Christian truths in the singularity, originality, and creativity of the person. In discovering their potential and accepting their limitations, they will direct their freedom with their conviction, reinforce their will with decisiveness and direct their friendship with the virtue of constancy in their day-to-day life, personally and with others.</p>
                                <p>The Cursillo method aims at creating small groups of Christians who will evangelize their environments with the Gospel spirit. This is achieved by means of a three-day weekend (Cursillo) which provides the tools for a life of ongoing spiritual growth.</p>
                                <h3>What is a Cursillo Weekend?</h3>
                                <p>It is an encounter with oneself, with Christ, and with others. It is a three-day experience of joy, study, and prayer.</p>",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    IsVisible = true,
                    ImageUrl = "https://images.unsplash.com/photo-1515162305285-0293e4767cc2?q=80&w=2671&auto=format&fit=crop" // Church/Cross interaction
                },
                new Article
                {
                    Title = "Upcoming Ultreya",
                    Content = @"<p>Join us for our monthly Ultreya meeting this Saturday at 7:00 PM. It is a time for sharing, prayer, and fellowship. Ultreya is the Spanish word for 'Onward!', encouraging us to persevere in our Christian walk.</p>
                                <p>We will have a guest speaker, Father John, who will talk about 'Living Grace in a Busy World'. Refreshments will be provided afterwards.</p>
                                <ul>
                                    <li><strong>Date:</strong> Saturday, February 10th</li>
                                    <li><strong>Time:</strong> 7:00 PM</li>
                                    <li><strong>Location:</strong> St. Mary's Parish Hall</li>
                                </ul>",
                    CreatedDate = DateTime.Now.AddDays(-2),
                    IsVisible = true,
                    ImageUrl = "https://images.unsplash.com/photo-1438232992991-995b7058bbb3?q=80&w=2673&auto=format&fit=crop"
                },
                new Article
                {
                    Title = "A Message from the Spiritual Director",
                    Content = @"<p>Dear Cursillistas,</p>
                                <p>As we enter this new season, let us reflect on the importance of piety, study, and action in our daily lives. These three legs of the tripod are essential for maintaining a balanced Christian life.</p>
                                <p><strong>Piety</strong> helps us to give our whole heart to God.<br>
                                <strong>Study</strong> helps us to give our whole mind to God.<br>
                                <strong>Action</strong> helps us to give our whole strength to God's service.</p>
                                <p>Let us continue to support one another in our Fourth Day.</p>",
                    CreatedDate = DateTime.Now,
                    IsVisible = true,
                    ImageUrl = "https://images.unsplash.com/photo-1504052434569-70ad5836ab65?q=80&w=2670&auto=format&fit=crop", // Book/Study
                    Order = 1
                },
                new Article
                {
                    Title = "School of Leaders",
                    Content = @"<p>The School of Leaders is where we deepen our understanding of the Cursillo mentality and method. It is open to all Cursillistas who wish to serve the movement.</p>
                                <p>Next session will cover the 'Technique of the Pre-Cursillo'. Understanding how to identify and prepare candidates is crucial for a successful weekend.</p>",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    IsVisible = true,
                    ImageUrl = "https://images.unsplash.com/photo-1524178232363-1fb2b075b655?q=80&w=2670&auto=format&fit=crop"
                }
            );
            await context.SaveChangesAsync();
        }

        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
        if (await userManager.FindByNameAsync("Admin") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "Admin",
                Email = "admin@local.test",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, ""); // Blank password
        }
    }
}
