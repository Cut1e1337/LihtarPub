using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;
using Lihtar.Infrastructure.Data;
using Lihtar.Infrastructure.Identity;
using Lihtar.Web.Helpers;
using Lihtar.Web.ViewModels.PublicEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Controllers;

public class PublicEventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly IEventCategoryService _categoryService;

    private readonly ArtPubDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public PublicEventsController(
        IEventService eventService,
        IEventCategoryService categoryService,
        ArtPubDbContext db,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender)
    {
        _eventService = eventService;
        _categoryService = categoryService;
        _db = db;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    // ---------------- INDEX ----------------
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

        all = all.OrderBy(x => x.EventDate).ToList();

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

    // ---------------- DETAILS ----------------
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var e = await _db.Events
            .Include(x => x.EventCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e is null) return NotFound();

        var vm = new PublicEventDetailsVm
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            CategoryName = e.EventCategory?.Name ?? "",
            EventDate = e.EventDate,
            DurationMinutes = e.DurationMinutes,
            Price = e.Price,
            TotalSeats = e.TotalSeats,
            AvailableSeats = e.AvailableSeats,
            ImageUrl = e.ImageUrl,
            IsActive = e.IsActive,
            CanBook = e.IsActive && e.EventDate >= DateTime.Now && e.AvailableSeats > 0
        };

        return View(vm);
    }

    // ---------------- BOOK (reserve seat) ----------------
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(Guid id)
    {
        var userIdStr = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return Forbid();

        var e = await _db.Events.FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NotFound();

        if (!e.IsActive || e.EventDate < DateTime.Now)
        {
            TempData["Error"] = "Подія недоступна для бронювання.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (e.AvailableSeats <= 0)
        {
            TempData["Error"] = "Немає вільних місць.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var already = await _db.EventTickets.AnyAsync(t =>
            t.EventId == id && t.UserId == userId && t.Status == TicketStatus.Active);

        if (already)
        {
            TempData["Error"] = "У тебе вже є активний квиток на цю подію.";
            return RedirectToAction(nameof(Details), new { id });
        }

        EventTicket? ticketForEmail = null;

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // ще раз перевірка (після початку транзакції)
            e = await _db.Events.FirstOrDefaultAsync(x => x.Id == id);
            if (e is null)
            {
                await tx.RollbackAsync();
                return NotFound();
            }

            if (e.AvailableSeats <= 0)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Місця вже закінчились.";
                return RedirectToAction(nameof(Details), new { id });
            }

            e.AvailableSeats -= 1;

            ticketForEmail = new EventTicket
            {
                Id = Guid.NewGuid(),
                EventId = e.Id,
                UserId = userId,
                Price = e.Price,
                PurchaseDate = DateTime.UtcNow,
                QRCode = $"EVT-{e.Id:N}-USR-{userId:N}-T-{Guid.NewGuid():N}",
                Status = TicketStatus.Active
            };

            _db.EventTickets.Add(ticketForEmail);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = "Квиток заброньовано ✅";
        }
        catch
        {
            await tx.RollbackAsync();
            TempData["Error"] = "Помилка бронювання. Спробуй ще раз.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ✅ Email після commit: QR як inline CID (працює навіть на localhost)
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email;

            if (!string.IsNullOrWhiteSpace(email) && ticketForEmail is not null)
            {
                var cid = "ticket-qr";
                var qrPngBytes = QrCodeHelper.ToPngBytes(ticketForEmail.QRCode);

                var subject = $"Lihtar — твій квиток: {e.Title}";
                var body = $@"
<h2>Квиток заброньовано ✅</h2>
<p><b>Подія:</b> {e.Title}</p>
<p><b>Дата:</b> {e.EventDate:dd.MM.yyyy HH:mm}</p>
<p><b>Ціна:</b> {e.Price} грн</p>
<p><b>Твій QR-код:</b></p>
<img src='cid:{cid}' style='width:260px;height:260px' />
<p style='margin-top:10px'>Код квитка: <b>{ticketForEmail.QRCode}</b></p>
";

                await _emailSender.SendAsync(
                    email,
                    subject,
                    body,
                    new List<EmailAttachment>
                    {
                        new EmailAttachment
                        {
                            FileName = "ticket-qr.png",
                            Content = qrPngBytes,
                            ContentType = "image/png",
                            IsInline = true,
                            ContentId = cid
                        }
                    });
            }
        }
        catch
        {
            // ігноруємо: квиток вже створений
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}