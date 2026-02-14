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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _menuItemService.GetAllAsync();
        return View(items);
    }

    // ---------- CREATE ----------

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new MenuItemEditVm { IsAvailable = true };

        await FillCategories();
        await FillTags(vm, new List<Guid>());

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemEditVm vm)
    {
        // ✅ валідація категорії (бо Guid дефолт = пустий)
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

    // ---------- EDIT ----------

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _menuItemService.GetByIdAsync(id);
        if (dto is null) return NotFound();

        var vm = new MenuItemEditVm
        {
            Id = dto.Id,
            MenuCategoryId = dto.MenuCategoryId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Calories = dto.Calories,
            WeightGrams = dto.WeightGrams,
            ImageUrl = dto.ImageUrl,
            IsAvailable = dto.IsAvailable
        };

        await FillCategories();
        await FillTags(vm, dto.TagIds ?? new List<Guid>());

        return View("Create", vm); // одна форма
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MenuItemEditVm vm)
    {
        if (vm.Id is null) return NotFound();

        if (vm.MenuCategoryId == Guid.Empty)
            ModelState.AddModelError(nameof(vm.MenuCategoryId), "Оберіть категорію");

        if (!ModelState.IsValid)
        {
            await FillCategories();
            await FillTags(vm, vm.SelectedTagIds);
            return View("Create", vm);
        }

        // ✅ якщо нове фото не вибрали — лишаємо старе
        var newImageUrl = await SaveImageAsync(vm.ImageFile);
        var finalImageUrl = newImageUrl ?? vm.ImageUrl;

        await _menuItemService.UpdateAsync(new MenuItemDto
        {
            Id = vm.Id.Value,
            MenuCategoryId = vm.MenuCategoryId,
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            Calories = vm.Calories,
            WeightGrams = vm.WeightGrams,
            ImageUrl = finalImageUrl,
            IsAvailable = vm.IsAvailable,
            TagIds = vm.SelectedTagIds
        });

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
        // ✅ НАПРЯМУ з БД (працює 100%)
        var categories = await _db.MenuCategories
            .AsNoTracking()
            .Where(x => x.IsActive) // якщо хочеш показувати тільки активні
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        // + placeholder
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
