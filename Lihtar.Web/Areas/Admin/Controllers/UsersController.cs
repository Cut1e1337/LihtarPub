using Lihtar.Infrastructure.Identity;
using Lihtar.Web.Areas.Admin.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 5) pageSize = 5;
        if (pageSize > 100) pageSize = 100;

        var usersQuery = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            usersQuery = usersQuery.Where(u =>
                (u.Email != null && u.Email.Contains(q)) ||
                (u.UserName != null && u.UserName.Contains(q)));
        }

        var total = await usersQuery.CountAsync();

        var users = await usersQuery
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserRowVm
            {
                Id = u.Id,
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                IsBlocked = u.IsBlocked,
                Roles = "" // заповнимо нижче
            })
            .ToListAsync();

        // ролі (окремо)
        foreach (var u in users)
        {
            var entity = await _userManager.FindByIdAsync(u.Id.ToString());
            if (entity != null)
            {
                var roles = await _userManager.GetRolesAsync(entity);
                u.Roles = string.Join(", ", roles);
            }
        }

        var vm = new UsersListVm
        {
            Q = q,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Users = users
        };

        return View(vm);
    }

    // ✅ Block / Unblock
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(Guid id, string? q, int page = 1, int pageSize = 20)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        // не даємо заблокувати самого себе
        var currentUserId = _userManager.GetUserId(User);
        if (!string.IsNullOrWhiteSpace(currentUserId) && currentUserId == user.Id.ToString())
        {
            TempData["Error"] = "Не можна заблокувати самого себе.";
            return RedirectToAction(nameof(Index), new { q, page, pageSize });
        }

        user.IsBlocked = !user.IsBlocked;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index), new { q, page, pageSize });
        }

        // ✅ force logout (корисно)
        await _userManager.UpdateSecurityStampAsync(user);

        TempData["Success"] = user.IsBlocked
            ? "Користувача заблоковано."
            : "Користувача розблоковано.";

        return RedirectToAction(nameof(Index), new { q, page, pageSize });
    }

    // ✅ Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, string? q, int page = 1, int pageSize = 20)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        // не даємо видалити самого себе
        var currentUserId = _userManager.GetUserId(User);
        if (!string.IsNullOrWhiteSpace(currentUserId) && currentUserId == user.Id.ToString())
        {
            TempData["Error"] = "Не можна видалити самого себе.";
            return RedirectToAction(nameof(Index), new { q, page, pageSize });
        }

        try
        {
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["Success"] = "Користувача видалено.";
            }
        }
        catch (DbUpdateException)
        {
            // якщо є FK на Orders/Reservations/Reviews і т.д.
            TempData["Error"] =
                "Не можу видалити користувача, бо є пов’язані дані (замовлення/бронювання/відгуки). Краще заблокувати.";
        }

        return RedirectToAction(nameof(Index), new { q, page, pageSize });
    }
}
