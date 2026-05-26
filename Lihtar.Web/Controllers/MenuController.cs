using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Lihtar.Infrastructure.Identity;
using Lihtar.Web.ViewModels.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Controllers;

public class MenuController : Controller
{
    private readonly ArtPubDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MenuController(ArtPubDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? q,
        Guid? categoryId,
        bool favoritesOnly = false,
        bool onlyAvailable = true,
        int page = 1,
        int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 6) pageSize = 6;
        if (pageSize > 48) pageSize = 48;

        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        Guid? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = _userManager.GetUserId(User);

            if (Guid.TryParse(userIdStr, out var parsedUserId))
                userId = parsedUserId;
        }

        if (favoritesOnly && !userId.HasValue)
            return RedirectToAction("Login", "Account");

        var favoriteIds = userId.HasValue
            ? await _db.FavoriteMenuItems
                .AsNoTracking()
                .Where(x => x.UserId == userId.Value)
                .Select(x => x.MenuItemId)
                .ToListAsync()
            : new List<Guid>();

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
                IsActive = !favoritesOnly && categoryId.HasValue && categoryId.Value == c.Id
            })
            .ToListAsync();

        if (userId.HasValue)
        {
            categories.Insert(0, new MenuCategoryFilterVm
            {
                Id = Guid.Empty,
                Name = "Улюблені",
                Count = favoriteIds.Count,
                IsActive = favoritesOnly
            });
        }

        var itemsQuery = _db.MenuItems
            .AsNoTracking()
            .Include(x => x.MenuCategory)
            .Include(x => x.TagLinks).ThenInclude(t => t.Tag)
            .Include(x => x.MenuItemReviews).ThenInclude(r => r.Review)
            .Where(x => x.MenuCategory != null && x.MenuCategory.IsActive);

        if (onlyAvailable)
            itemsQuery = itemsQuery.Where(x => x.IsAvailable);

        if (favoritesOnly && userId.HasValue)
            itemsQuery = itemsQuery.Where(x => favoriteIds.Contains(x.Id));

        if (!favoritesOnly && categoryId.HasValue)
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
                Tags = x.TagLinks.Select(t => t.Tag!.Name).ToList(),

                ReviewsCount = x.MenuItemReviews.Count(r => r.Review != null && r.Review.IsApproved),

                AverageRating = x.MenuItemReviews
                    .Where(r => r.Review != null && r.Review.IsApproved)
                    .Select(r => (double?)r.Review!.Rating)
                    .Average() ?? 0
            })
            .ToListAsync();

        foreach (var item in items)
            item.IsFavorite = favoriteIds.Contains(item.Id);

        var vm = new MenuIndexVm
        {
            Q = q,
            CategoryId = categoryId,
            FavoritesOnly = favoritesOnly,
            OnlyAvailable = onlyAvailable,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Categories = categories,
            Items = items
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        Guid? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = _userManager.GetUserId(User);

            if (Guid.TryParse(userIdStr, out var parsedUserId))
                userId = parsedUserId;
        }

        var item = await _db.MenuItems
            .AsNoTracking()
            .Include(x => x.MenuCategory)
            .Include(x => x.TagLinks).ThenInclude(t => t.Tag)
            .Include(x => x.MenuItemReviews).ThenInclude(r => r.Review)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return NotFound();

        var reviewUserIds = item.MenuItemReviews
            .Where(x => x.Review != null && x.Review.IsApproved)
            .Select(x => x.Review!.UserId)
            .Distinct()
            .ToList();

        var users = await _userManager.Users
            .AsNoTracking()
            .Where(x => reviewUserIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.Email
            })
            .ToListAsync();

        var profiles = await _db.UserProfiles
            .AsNoTracking()
            .Where(x => reviewUserIds.Contains(x.UserId))
            .Select(x => new
            {
                x.UserId,
                x.AvatarUrl
            })
            .ToListAsync();

        var reviews = item.MenuItemReviews
            .Where(x => x.Review != null && x.Review.IsApproved)
            .OrderByDescending(x => x.Review!.CreatedAt)
            .Select(x =>
            {
                var review = x.Review!;
                var user = users.FirstOrDefault(u => u.Id == review.UserId);
                var profile = profiles.FirstOrDefault(p => p.UserId == review.UserId);

                var email = user?.Email ?? "Користувач";

                return new MenuReviewVm
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    UserEmail = email,
                    UserFullName = email,
                    UserAvatarUrl = profile?.AvatarUrl,
                    Rating = review.Rating,
                    Text = review.Text,
                    CreatedAt = review.CreatedAt,
                    IsMine = userId.HasValue && review.UserId == userId.Value
                };
            })
            .ToList();

        var vm = new MenuItemDetailsVm
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Calories = item.Calories,
            WeightGrams = item.WeightGrams,
            ImageUrl = item.ImageUrl,
            IsAvailable = item.IsAvailable,
            CategoryName = item.MenuCategory?.Name ?? "",
            Tags = item.TagLinks.Select(x => x.Tag!.Name).ToList(),
            Reviews = reviews,
            ReviewsCount = reviews.Count,
            AverageRating = reviews.Count == 0 ? 0 : reviews.Average(x => x.Rating),
            IsFavorite = userId.HasValue &&
                         await _db.FavoriteMenuItems.AnyAsync(x =>
                             x.UserId == userId.Value &&
                             x.MenuItemId == id)
        };

        return View(vm);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(Guid menuItemId)
    {
        var userIdStr = _userManager.GetUserId(User);

        if (!Guid.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account");

        var exists = await _db.MenuItems.AnyAsync(x => x.Id == menuItemId);

        if (!exists)
            return NotFound();

        var favorite = await _db.FavoriteMenuItems
            .FirstOrDefaultAsync(x => x.UserId == userId && x.MenuItemId == menuItemId);

        if (favorite == null)
        {
            _db.FavoriteMenuItems.Add(new FavoriteMenuItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MenuItemId = menuItemId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["Success"] = "Страву додано в улюблені.";
        }
        else
        {
            _db.FavoriteMenuItems.Remove(favorite);
            TempData["Success"] = "Страву видалено з улюблених.";
        }

        await _db.SaveChangesAsync();

        var referer = Request.Headers.Referer.ToString();

        if (!string.IsNullOrWhiteSpace(referer))
            return Redirect(referer);

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(Guid menuItemId, int rating, string text)
    {
        var userIdStr = _userManager.GetUserId(User);

        if (!Guid.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account");

        if (rating < 1 || rating > 5)
        {
            TempData["Error"] = "Оцінка має бути від 1 до 5.";
            return RedirectToAction(nameof(Details), new { id = menuItemId });
        }

        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 3)
        {
            TempData["Error"] = "Текст відгуку має містити мінімум 3 символи.";
            return RedirectToAction(nameof(Details), new { id = menuItemId });
        }

        var itemExists = await _db.MenuItems.AnyAsync(x => x.Id == menuItemId);

        if (!itemExists)
            return NotFound();

        var alreadyReviewed = await _db.MenuItemReviews
            .AnyAsync(x =>
                x.MenuItemId == menuItemId &&
                x.Review != null &&
                x.Review.UserId == userId);

        if (alreadyReviewed)
        {
            TempData["Error"] = "Ви вже залишали відгук на цю страву.";
            return RedirectToAction(nameof(Details), new { id = menuItemId });
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Rating = rating,
            Text = text.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsApproved = true
        };

        _db.Reviews.Add(review);

        _db.MenuItemReviews.Add(new MenuItemReview
        {
            ReviewId = review.Id,
            MenuItemId = menuItemId
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Відгук додано.";
        return RedirectToAction(nameof(Details), new { id = menuItemId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(Guid reviewId, Guid menuItemId)
    {
        var userIdStr = _userManager.GetUserId(User);

        if (!Guid.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account");

        var review = await _db.Reviews
            .FirstOrDefaultAsync(x => x.Id == reviewId && x.UserId == userId);

        if (review == null)
            return NotFound();

        var link = await _db.MenuItemReviews
            .FirstOrDefaultAsync(x => x.ReviewId == reviewId && x.MenuItemId == menuItemId);

        if (link != null)
            _db.MenuItemReviews.Remove(link);

        _db.Reviews.Remove(review);

        await _db.SaveChangesAsync();

        TempData["Success"] = "Відгук видалено.";
        return RedirectToAction(nameof(Details), new { id = menuItemId });
    }
}