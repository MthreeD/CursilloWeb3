using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CursilloWeb.Data;

[Table("Page2")]
public class Page2Data
{
    [Key]
    public Guid Id { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? HTMLContents { get; set; }
}