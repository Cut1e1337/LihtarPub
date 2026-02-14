using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Lihtar.Web.ViewModels.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Areas.Admin.Controllers;

public class EventsController : AdminBaseController
{
    private readonly ArtPubDbContext _db;
    private readonly IWebHostEnvironment _env;

    public EventsController(ArtPubDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Events
            .Include(x => x.EventCategory)
            .AsNoTracking()
            .OrderByDescending(x => x.EventDate)
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await FillCategories();

        var vm = new EventEditVm
        {
            IsActive = true,
            EventDate = DateTime.Now.AddDays(1),
            DurationMinutes = 60,
            TotalSeats = 20
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventEditVm vm)
    {
        ValidateEventVm(vm);

        if (!ModelState.IsValid)
        {
            await FillCategories();
            return View(vm);
        }

        var imageUrl = await SaveImageAsync(vm.ImageFile);
        if (vm.ImageFile is not null && imageUrl is null)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), "Дозволено тільки jpg/jpeg/png/webp");
            await FillCategories();
            return View(vm);
        }

        var entity = new Event
        {
            Id = Guid.NewGuid(),
            EventCategoryId = vm.EventCategoryId,
            Title = vm.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
            EventDate = vm.EventDate,
            DurationMinutes = vm.DurationMinutes,
            Price = vm.Price,
            TotalSeats = vm.TotalSeats,
            AvailableSeats = vm.TotalSeats,
            ImageUrl = imageUrl,
            IsActive = vm.IsActive
        };

        _db.Events.Add(entity);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _db.Events
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null) return NotFound();

        await FillCategories();

        var vm = new EventEditVm
        {
            Id = entity.Id,
            EventCategoryId = entity.EventCategoryId,
            Title = entity.Title,
            Description = entity.Description,
            EventDate = entity.EventDate,
            DurationMinutes = entity.DurationMinutes,
            Price = entity.Price,
            TotalSeats = entity.TotalSeats,
            AvailableSeats = entity.AvailableSeats,
            ImageUrl = entity.ImageUrl,
            IsActive = entity.IsActive
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventEditVm vm)
    {
        if (vm.Id is null) return NotFound();

        ValidateEventVm(vm);

        var entity = await _db.Events
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == vm.Id.Value);

        if (entity is null) return NotFound();

        // seats safety: TotalSeats >= sold
        var sold = entity.TotalSeats - entity.AvailableSeats;
        if (vm.TotalSeats < sold)
            ModelState.AddModelError(nameof(vm.TotalSeats), $"Вже продано {sold} квитків. TotalSeats не може бути меншим.");

        if (!ModelState.IsValid)
        {
            await FillCategories();
            vm.ImageUrl = entity.ImageUrl;
            vm.AvailableSeats = entity.AvailableSeats;
            return View(vm);
        }

        // upload (optional)
        var newImageUrl = await SaveImageAsync(vm.ImageFile);
        if (vm.ImageFile is not null && newImageUrl is null)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), "Дозволено тільки jpg/jpeg/png/webp");
            await FillCategories();
            vm.ImageUrl = entity.ImageUrl;
            vm.AvailableSeats = entity.AvailableSeats;
            return View(vm);
        }

        var finalImageUrl = newImageUrl ?? entity.ImageUrl;

        entity.EventCategoryId = vm.EventCategoryId;
        entity.Title = vm.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
        entity.EventDate = vm.EventDate;
        entity.DurationMinutes = vm.DurationMinutes;
        entity.Price = vm.Price;

        entity.TotalSeats = vm.TotalSeats;
        entity.AvailableSeats = vm.TotalSeats - sold;

        entity.ImageUrl = finalImageUrl;
        entity.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Events
            .Include(x => x.EventCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null) return NotFound();
        return View(entity);
    }

    // ✅ важливо: інша назва, щоб не було плутанини
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var entity = await _db.Events
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null) return RedirectToAction(nameof(Index));

        if (entity.Tickets.Any())
        {
            TempData["Error"] = "Неможливо видалити подію: є куплені квитки.";
            return RedirectToAction(nameof(Index));
        }

        _db.Events.Remove(entity);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateEventVm(EventEditVm vm)
    {
        if (vm.EventCategoryId == Guid.Empty)
            ModelState.AddModelError(nameof(vm.EventCategoryId), "Оберіть категорію");

        if (string.IsNullOrWhiteSpace(vm.Title))
            ModelState.AddModelError(nameof(vm.Title), "Title is required");

        if (vm.TotalSeats <= 0)
            ModelState.AddModelError(nameof(vm.TotalSeats), "TotalSeats must be > 0");

        if (vm.DurationMinutes <= 0)
            ModelState.AddModelError(nameof(vm.DurationMinutes), "DurationMinutes must be > 0");

        if (vm.Price < 0)
            ModelState.AddModelError(nameof(vm.Price), "Price must be >= 0");

        if (vm.EventDate < DateTime.Now)
            ModelState.AddModelError(nameof(vm.EventDate), "Дата події має бути в майбутньому");
    }

    private async Task FillCategories()
    {
        var categories = await _db.EventCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        categories.Insert(0, new SelectListItem { Value = "", Text = "-- Select category --" });
        ViewBag.Categories = categories;
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext)) return null;

        var folder = Path.Combine(_env.WebRootPath, "uploads", "events");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return $"/uploads/events/{fileName}";
    }
}
