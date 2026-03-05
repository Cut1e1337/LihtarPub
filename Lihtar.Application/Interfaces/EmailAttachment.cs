namespace Lihtar.Application.Interfaces;

public class EmailAttachment
{
    public string FileName { get; set; } = default!;
    public byte[] Content { get; set; } = default!;
    public string ContentType { get; set; } = "application/octet-stream";

    // якщо true — вставляємо в лист як картинку (CID)
    public bool IsInline { get; set; } = false;
    public string? ContentId { get; set; }
}