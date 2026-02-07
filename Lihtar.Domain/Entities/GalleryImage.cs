namespace Lihtar.Domain.Entities;

public class GalleryImage
{
    public Guid Id { get; set; }
    public Guid AlbumId { get; set; }
    public GalleryAlbum? Album { get; set; }

    public string ImageUrl { get; set; } = default!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
