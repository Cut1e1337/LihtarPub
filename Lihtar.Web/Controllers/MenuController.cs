using Lihtar.Infrastructure.Data;
using Lihtar.Web.ViewModels.Menu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Controllers;

public class MenuController : Controller
{
    private readonly ArtPubDbContext _db;

    public MenuController(ArtPubDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, Guid? categoryId, bool onlyAvailable = true, int page = 1, int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 6) pageSize = 6;
        if (pageSize > 48) pageSize = 48;

        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        // Категорії (з лічильником позицій)
        var categories = await _db.MenuCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new MenuCategoryFilterVm
            {
                Id = c.Id,
                Name = c.Name,
                Count = c.Items.Count(i => !onlyAvailable || i.IsAvailable),
                IsActive = categoryId.HasValue && categoryId.Value == c.Id
            })
            .ToListAsync();

        // Запит по позиціях
        var itemsQuery = _db.MenuItems
            .AsNoTracking()
            .Include(x => x.MenuCategory)
            .Include(x => x.TagLinks).ThenInclude(t => t.Tag)
            .Where(x => x.MenuCategory != null && x.MenuCategory.IsActive);

        if (onlyAvailable)
            itemsQuery = itemsQuery.Where(x => x.IsAvailable);

        if (categoryId.HasValue)
            itemsQuery = itemsQuery.Where(x => x.MenuCategoryId == categoryId.Value);

        if (q != null)
            itemsQuery = itemsQuery.Where(x => x.Name.Contains(q));

        var total = await itemsQuery.CountAsync();

        var items = await itemsQuery
            .OrderBy(x => x.MenuCategory!.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MenuItemCardVm
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Calories = x.Calories,
                WeightGrams = x.WeightGrams,
                ImageUrl = x.ImageUrl,
                IsAvailable = x.IsAvailable,
                CategoryId = x.MenuCategoryId,
                CategoryName = x.MenuCategory!.Name,
                Tags = x.TagLinks.Select(t => t.Tag!.Name).ToList()
            })
            .ToListAsync();

        var vm = new MenuIndexVm
        {
            Q = q,
            CategoryId = categoryId,
            OnlyAvailable = onlyAvailable,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Categories = categories,
            Items = items
        };

        return View(vm);
    }
}
