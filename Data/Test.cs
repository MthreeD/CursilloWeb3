using System.ComponentModel.DataAnnotations;

namespace CursilloWeb.Data;

public class Test
{
    public Guid Id { get; set; }

    public string? BackgroundColor { get; set; }

    public string? FontName { get; set; }

    public string? FontSize { get; set; }

    public byte[]? DocumentContent { get; set; }

    public byte[]? RTFContent { get; set; }

    public string? HTMLContent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}