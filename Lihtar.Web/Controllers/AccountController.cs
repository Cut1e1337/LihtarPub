using Lihtar.Domain.Enums;
using Lihtar.Infrastructure.Identity;
using Lihtar.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginVm { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user is null)
        {
            ModelState.AddModelError("", "Невірний email або пароль");
            return View(vm);
        }

        if (user.IsBlocked)
        {
            ModelState.AddModelError("", "Акаунт заблоковано");
            return View(vm);
        }

        // userName == email, тому логінимось по email
        var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Забагато спроб. Спробуй пізніше.");
            return View(vm);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Невірний email або пароль");
            return View(vm);
        }

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(UserRole.Admin.ToString()))
            return RedirectToAction("Index", "Home", new { area = "Admin" });

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var existing = await _userManager.FindByEmailAsync(vm.Email);
        if (existing is not null)
        {
            ModelState.AddModelError("", "Користувач з таким email вже існує");
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FullName = vm.FullName,
            Role = UserRole.Client,
            EmailConfirmed = true, // ✅ щоб без SMTP все працювало
            IsBlocked = false
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);

            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
