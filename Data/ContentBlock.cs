using System.ComponentModel.DataAnnotations;

namespace CursilloWeb.Data;

public class ContentBlock
{
    public Guid Id { get; set; }

    [Required]
    public string Section { get; set; } = string.Empty; // e.g. "Header", "Footer"

    public string? HtmlContent { get; set; }

    public string? RichTextContent { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
