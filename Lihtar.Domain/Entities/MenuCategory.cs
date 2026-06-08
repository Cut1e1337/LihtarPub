namespace Lihtar.Domain.Entities;

public class MenuCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
