using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Lihtar.Infrastructure.Identity;
using Lihtar.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ArtPubDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        ArtPubDbContext db,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == user.Id);

        if (profile == null)
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BonusPoints = 0
            };

            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }

        var vm = new ProfileVm
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            FullName = user.FullName,
            BirthDate = profile.BirthDate,
            AvatarUrl = profile.AvatarUrl,
            BonusPoints = profile.BonusPoints
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileVm vm)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == user.Id);

        if (profile == null)
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BonusPoints = 0
            };

            _db.UserProfiles.Add(profile);
        }

        if (!ModelState.IsValid)
        {
            vm.Email = user.Email ?? "";
            vm.AvatarUrl = profile.AvatarUrl;
            vm.BonusPoints = profile.BonusPoints;
            return View(vm);
        }

        user.FullName = vm.FullName;
        profile.BirthDate = vm.BirthDate;

        if (vm.AvatarFile != null && vm.AvatarFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "avatars");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(vm.AvatarFile.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await vm.AvatarFile.CopyToAsync(stream);

            profile.AvatarUrl = $"/uploads/avatars/{fileName}";
        }

        await _userManager.UpdateAsync(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Профіль оновлено.";

        return RedirectToAction(nameof(Index));
    }
}