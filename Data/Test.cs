using System.ComponentModel.DataAnnotations;

namespace CursilloWeb.Data;

public class Test
{
    public Guid Id { get; set; }

    public string? TextBoxText { get; set; }

    public string? HtmlTest { get; set; }

    public string? FontName { get; set; }

    public string? FontSize { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}