namespace Lihtar.Web.ViewModels.MenuItems;

public class MenuItemEditVm
{
    public Guid? Id { get; set; }
    public Guid MenuCategoryId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public int? Calories { get; set; }
    public int? WeightGrams { get; set; }

    // ✅ для збереженого в БД посилання
    public string? ImageUrl { get; set; }

    // ✅ для завантаження файлу з форми
    public IFormFile? ImageFile { get; set; }

    public bool IsAvailable { get; set; } = true;

    public List<TagCheckboxVm> Tags { get; set; } = new();

    // ✅ тільки GET, не присвоюємо
    public List<Guid> SelectedTagIds => Tags.Where(x => x.Selected).Select(x => x.Id).ToList();
}

public class TagCheckboxVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool Selected { get; set; }
}
