namespace Lihtar.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; set; }
    public Guid MenuCategoryId { get; set; }
    public MenuCategory? MenuCategory { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public int? Calories { get; set; }
    public int? WeightGrams { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;

    public ICollection<MenuItemTagLink> TagLinks { get; set; } = new List<MenuItemTagLink>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<MenuItemReview> MenuItemReviews { get; set; } = new List<MenuItemReview>();
    public ICollection<FavoriteMenuItem> Favorites { get; set; } = new List<FavoriteMenuItem>();
}
