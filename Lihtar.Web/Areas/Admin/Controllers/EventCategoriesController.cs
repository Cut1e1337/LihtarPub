using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Lihtar.Web.ViewModels.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Areas.Admin.Controllers;

public class EventCategoriesController : AdminBaseController
{
    private readonly ArtPubDbContext _db;
    public EventCategoriesController(ArtPubDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _db.EventCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
        => View(new EventCategoryEditVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventCategoryEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var entity = new EventCategory
            {
                Id = Guid.NewGuid(),
                Name = vm.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim()
            };

            _db.EventCategories.Add(entity);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Категорію подій додано ✅";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
            return View(vm);
        }
    }

    // ✅ /Admin/EventCategories/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _db.EventCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        return View(new EventCategoryEditVm
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventCategoryEditVm vm)
    {
        if (vm.Id is null || vm.Id == Guid.Empty) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var entity = await _db.EventCategories.FirstOrDefaultAsync(x => x.Id == vm.Id.Value);
            if (entity is null) return NotFound();

            entity.Name = vm.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] = "Категорію оновлено ✅";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.InnerException?.Message ?? ex.Message);
            return View(vm);
        }
    }

    // ✅ /Admin/EventCategories/Delete/{id}
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.EventCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        return View(entity);
    }

    // ✅ POST на той самий Delete (без DeleteConfirmed — простіше і надійніше)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, string? _)
    {
        var entity = await _db.EventCategories.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return RedirectToAction(nameof(Index));

        var hasEvents = await _db.Events.AnyAsync(e => e.EventCategoryId == id);
        if (hasEvents)
        {
            TempData["Error"] = "Неможливо видалити категорію: у ній є події.";
            return RedirectToAction(nameof(Index));
        }

        _db.EventCategories.Remove(entity);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Категорію видалено ✅";
        return RedirectToAction(nameof(Index));
    }
}
