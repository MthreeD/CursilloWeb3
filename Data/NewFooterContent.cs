namespace CursilloWeb.Data;

public class NewFooterContent
{
    public Guid Id { get; set; }

    public byte[]? RTFContent { get; set; }

    public byte[]? HTMLcode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
