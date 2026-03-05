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

    // ---------------- CREATE ----------------

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new EventEditVm
        {
            IsActive = true,
            EventDate = DateTime.Now.AddDays(1),
            DurationMinutes = 60,
            TotalSeats = 20,
            Price = 0
        };

        await PopulateCategories(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventEditVm vm)
    {
        ValidateEventVm(vm);

        if (!ModelState.IsValid)
        {
            await PopulateCategories(vm);
            return View(vm);
        }

        var imageUrl = await SaveImageAsync(vm.ImageFile);
        if (vm.ImageFile is not null && imageUrl is null)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), "Дозволено тільки jpg/jpeg/png/webp");
            await PopulateCategories(vm);
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

        TempData["Success"] = "Подію створено ✅";
        return RedirectToAction(nameof(Index));
    }

    // ---------------- EDIT ----------------

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _db.Events
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null) return NotFound();

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

        await PopulateCategories(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventEditVm vm)
    {
        if (vm.Id is null) return NotFound();

        var entity = await _db.Events
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == vm.Id.Value);

        if (entity is null) return NotFound();

        // seats safety: TotalSeats >= sold
        var sold = entity.TotalSeats - entity.AvailableSeats;

        ValidateEventVm(vm);
        if (vm.TotalSeats < sold)
            ModelState.AddModelError(nameof(vm.TotalSeats),
                $"Вже продано {sold} квитків. TotalSeats не може бути меншим.");

        if (!ModelState.IsValid)
        {
            vm.ImageUrl = entity.ImageUrl;
            vm.AvailableSeats = entity.AvailableSeats;
            await PopulateCategories(vm);
            return View(vm);
        }

        // upload (optional)
        var newImageUrl = await SaveImageAsync(vm.ImageFile);
        if (vm.ImageFile is not null && newImageUrl is null)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), "Дозволено тільки jpg/jpeg/png/webp");
            vm.ImageUrl = entity.ImageUrl;
            vm.AvailableSeats = entity.AvailableSeats;
            await PopulateCategories(vm);
            return View(vm);
        }

        entity.EventCategoryId = vm.EventCategoryId;
        entity.Title = vm.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
        entity.EventDate = vm.EventDate;
        entity.DurationMinutes = vm.DurationMinutes;
        entity.Price = vm.Price;

        entity.TotalSeats = vm.TotalSeats;
        entity.AvailableSeats = vm.TotalSeats - sold;

        entity.ImageUrl = newImageUrl ?? entity.ImageUrl;
        entity.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Подію оновлено ✅";
        return RedirectToAction(nameof(Index));
    }

    // ---------------- DELETE ----------------

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var entity = await _db.Events
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null) return RedirectToAction(nameof(Index));

        // ✅ видаляємо квитки події
        if (entity.Tickets.Any())
            _db.EventTickets.RemoveRange(entity.Tickets);

        _db.Events.Remove(entity);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Подію видалено ✅ (разом з квитками)";
        return RedirectToAction(nameof(Index));
    }

    // ---------------- helpers ----------------

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

        // в Edit дозволяємо минулу дату? якщо ні — залишай як є
        if (vm.EventDate < DateTime.Now)
            ModelState.AddModelError(nameof(vm.EventDate), "Дата події має бути в майбутньому");
    }

    private async Task PopulateCategories(EventEditVm vm)
    {
        vm.Categories = await _db.EventCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        vm.Categories.Insert(0, new SelectListItem { Value = "", Text = "-- Select category --" });
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
