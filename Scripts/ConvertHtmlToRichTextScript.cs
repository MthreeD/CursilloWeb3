using CursilloWeb.Data;
using CursilloWeb.DataMigration;
using Microsoft.EntityFrameworkCore;

namespace CursilloWeb.Scripts;

public class ConvertHtmlToRichTextScript
{
    public static async Task RunAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Starting HTML to RichText conversion...");

        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var converter = new HtmlToRichTextConverter(dbContextFactory);

        try
        {
            await converter.ConvertAllHtmlToRichTextAsync();
            Console.WriteLine("Conversion completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during conversion: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}