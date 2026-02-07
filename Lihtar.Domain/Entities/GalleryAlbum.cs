namespace Lihtar.Domain.Entities;

public class GalleryAlbum
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GalleryImage> Images { get; set; } = new List<GalleryImage>();
}
