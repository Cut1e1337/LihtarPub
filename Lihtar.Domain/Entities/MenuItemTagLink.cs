namespace Lihtar.Domain.Entities;

public class MenuItemTagLink
{
    public Guid MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    public Guid TagId { get; set; }
    public MenuItemTag? Tag { get; set; }
}
