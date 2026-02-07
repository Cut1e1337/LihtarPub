namespace Lihtar.Domain.Entities;

public class FavoriteMenuItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public Guid MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
