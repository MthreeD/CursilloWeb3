using System.ComponentModel.DataAnnotations;

namespace CursilloWeb.Data;

public class Article
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    public string? ImageUrl { get; set; }
    
    public bool IsVisible { get; set; } = true;
    
    public int Order { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
