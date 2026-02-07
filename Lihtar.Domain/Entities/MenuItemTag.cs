namespace Lihtar.Domain.Entities;

public class MenuItemTag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!; // Vegan, Spicy, Alcohol, etc.

    public ICollection<MenuItemTagLink> MenuItemLinks { get; set; } = new List<MenuItemTagLink>();
}
