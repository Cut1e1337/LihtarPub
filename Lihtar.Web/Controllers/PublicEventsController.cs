using Lihtar.Application.Interfaces;
using Lihtar.Web.ViewModels.PublicEvents;
using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Controllers;

public class PublicEventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly IEventCategoryService _categoryService;

    public PublicEventsController(IEventService eventService, IEventCategoryService categoryService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? q,
        Guid? categoryId,
        bool onlyUpcoming = true,
        bool onlyActive = true,
        int page = 1,
        int pageSize = 12)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 6 or > 48 ? 12 : pageSize;

        var all = await _eventService.GetAllAsync();
        var now = DateTime.Now;

        if (onlyActive)
            all = all.Where(x => x.IsActive).ToList();

        if (onlyUpcoming)
            all = all.Where(x => x.EventDate >= now).ToList();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qq = q.Trim().ToLowerInvariant();
            all = all.Where(x =>
                    (x.Title ?? "").ToLowerInvariant().Contains(qq) ||
                    (x.Description ?? "").ToLowerInvariant().Contains(qq) ||
                    (x.CategoryName ?? "").ToLowerInvariant().Contains(qq)
                )
                .ToList();
        }

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            all = all.Where(x => x.EventCategoryId == categoryId.Value).ToList();

        // категорії (тільки з контентом після фільтрів)
        var cats = await _categoryService.GetAllAsync();
        var counters = all.GroupBy(x => x.EventCategoryId)
            .ToDictionary(g => g.Key, g => g.Count());

        var categoryVms = cats
            .OrderBy(x => x.Name)
            .Select(c => new PublicEventCategoryFilterVm
            {
                Id = c.Id,
                Name = c.Name,
                Count = counters.TryGetValue(c.Id, out var cnt) ? cnt : 0
            })
            .Where(x => x.Count > 0)
            .ToList();

        // сортування найближчі першими
        all = all.OrderBy(x => x.EventDate).ToList();

        // paging
        var total = all.Count;
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        if (totalPages == 0) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PublicEventCardVm
            {
                Id = x.Id,
                Title = x.Title,
                CategoryName = x.CategoryName,
                EventDate = x.EventDate,
                DurationMinutes = x.DurationMinutes,
                Price = x.Price,
                TotalSeats = x.TotalSeats,
                AvailableSeats = x.AvailableSeats,
                ImageUrl = x.ImageUrl
            })
            .ToList();

        var vm = new PublicEventsIndexVm
        {
            Q = q,
            CategoryId = categoryId,
            OnlyUpcoming = onlyUpcoming,
            OnlyActive = onlyActive,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            Categories = categoryVms,
            Items = items
        };

        return View(vm);
    }

    [HttpGet("events/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var x = await _eventService.GetByIdAsync(id);
        if (x is null) return NotFound();

        return View(x); // зробимо потім (поки можна не створювати)
    }
}
