using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Infrastructure.Data;
using Lihtar.Web.ViewModels.MenuItems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Areas.Admin.Controllers;

public class MenuItemsController : AdminBaseController
{
    private readonly IMenuItemService _menuItemService;
    private readonly IMenuItemTagService _tagService;
    private readonly ArtPubDbContext _db;
    private readonly IWebHostEnvironment _env;

    public MenuItemsController(
        IMenuItemService menuItemService,
        IMenuItemTagService tagService,
        ArtPubDbContext db,
        IWebHostEnvironment env)
    {
        _menuItemService = menuItemService;
        _tagService = tagService;
        _db = db;
        _env = env;
    }

    // ---------- INDEX ----------
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _menuItemService.GetAllAsync();
        return View(items);
    }

    // ---------- CREATE GET ----------
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new MenuItemEditVm { IsAvailable = true };

        await FillCategories();
        await FillTags(vm, new List<Guid>());

        return View(vm); // Views/Admin/MenuItems/Create.cshtml
    }

    // ---------- CREATE POST ----------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemEditVm vm)
    {
        if (vm.MenuCategoryId == Guid.Empty)
            ModelState.AddModelError(nameof(vm.MenuCategoryId), "Оберіть категорію");

        if (!ModelState.IsValid)
        {
            await FillCategories();
            await FillTags(vm, vm.SelectedTagIds);
            return View(vm);
        }

        var imageUrl = await SaveImageAsync(vm.ImageFile);

        await _menuItemService.CreateAsync(new MenuItemDto
        {
            MenuCategoryId = vm.MenuCategoryId,
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            Calories = vm.Calories,
            WeightGrams = vm.WeightGrams,
            ImageUrl = imageUrl,
            IsAvailable = vm.IsAvailable,
            TagIds = vm.SelectedTagIds
        });

        return RedirectToAction(nameof(Index));
    }

    // ---------- EDIT GET ----------
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var item = await _db.MenuItems
            .Include(x => x.TagLinks)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound();

        var vm = new MenuItemEditVm
        {
            Id = item.Id,
            MenuCategoryId = item.MenuCategoryId,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Calories = item.Calories,
            WeightGrams = item.WeightGrams,
            ImageUrl = item.ImageUrl,
            IsAvailable = item.IsAvailable
        };

        await FillCategories();

        var selectedTagIds = item.TagLinks
            .Select(x => x.TagId)
            .ToList();

        await FillTags(vm, selectedTagIds);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MenuItemEditVm vm)
    {
        if (vm.Id is null) return NotFound();

        if (vm.MenuCategoryId == Guid.Empty)
            ModelState.AddModelError(nameof(vm.MenuCategoryId), "Оберіть категорію");

        var selectedTagIds = vm.Tags
            .Where(t => t.Selected)
            .Select(t => t.Id)
            .Distinct()
            .ToList();

        if (!ModelState.IsValid)
        {
            await FillCategories();
            await FillTags(vm, selectedTagIds);
            return View(vm);
        }

        var entity = await _db.MenuItems
            .Include(x => x.TagLinks)
            .FirstOrDefaultAsync(x => x.Id == vm.Id.Value);

        if (entity is null) return NotFound();

        var newImageUrl = await SaveImageAsync(vm.ImageFile);

        entity.MenuCategoryId = vm.MenuCategoryId;
        entity.Name = vm.Name;
        entity.Description = vm.Description;
        entity.Price = vm.Price;
        entity.Calories = vm.Calories;
        entity.WeightGrams = vm.WeightGrams;
        entity.IsAvailable = vm.IsAvailable;

        if (newImageUrl != null)
            entity.ImageUrl = newImageUrl;

        var removeTags = entity.TagLinks
            .Where(x => !selectedTagIds.Contains(x.TagId))
            .ToList();

        _db.MenuItemTagLinks.RemoveRange(removeTags);

        var existingTags = entity.TagLinks
            .Select(x => x.TagId)
            .ToHashSet();

        foreach (var tagId in selectedTagIds)
        {
            if (!existingTags.Contains(tagId))
            {
                entity.TagLinks.Add(new Lihtar.Domain.Entities.MenuItemTagLink
                {
                    MenuItemId = entity.Id,
                    TagId = tagId
                });
            }
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ---------- DELETE ----------
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _menuItemService.GetByIdAsync(id);
        if (dto is null) return NotFound();
        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _menuItemService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // ---------- HELPERS ----------
    private async Task FillCategories()
    {
        var categories = await _db.MenuCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        categories.Insert(0, new SelectListItem { Value = "", Text = "-- Select category --" });
        ViewBag.Categories = categories;
    }

    private async Task FillTags(MenuItemEditVm vm, List<Guid> selectedIds)
    {
        selectedIds ??= new List<Guid>();

        var tags = await _tagService.GetAllAsync();
        vm.Tags = (tags ?? new List<MenuItemTagDto>())
            .Select(t => new TagCheckboxVm
            {
                Id = t.Id,
                Name = t.Name,
                Selected = selectedIds.Contains(t.Id)
            })
            .ToList();
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext)) return null;

        var folder = Path.Combine(_env.WebRootPath, "uploads", "menu");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return $"/uploads/menu/{fileName}";
    }
}
