using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CursilloWeb.Data;

public class WebmasterSetting
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FullPath { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string FileExtensions { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
}