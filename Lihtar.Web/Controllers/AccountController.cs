using Lihtar.Application.Interfaces;
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
    private readonly IEmailSender _emailSender;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }

    // -------------------- LOGIN --------------------

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

        // login by username (у нас username == email)
        var result = await _signInManager.PasswordSignInAsync(
            userName: vm.Email,
            password: vm.Password,
            isPersistent: vm.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Забагато спроб. Спробуй пізніше.");
            return View(vm);
        }

        // ВАЖЛИВО: коли RequireConfirmedEmail=true, то сюди попаде непідтверджений юзер
        if (result.IsNotAllowed)
        {
            ModelState.AddModelError("", "Email не підтверджено. Перевір пошту і натисни Confirm.");
            return View(vm);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Невірний email або пароль");
            return View(vm);
        }

        // redirect returnUrl якщо є
        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        // redirect по ролі
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(UserRole.Admin.ToString()))
            return RedirectToAction("Index", "AdminHome", new { area = "Admin" });

        return RedirectToAction("Index", "Home");
    }

    // -------------------- REGISTER --------------------

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
            EmailConfirmed = false,   // ✅ тепер треба підтвердження
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

        // 1) token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // 2) link
        var link = Url.Action(
            nameof(ConfirmEmail),
            "Account",
            new { userId = user.Id, token },
            protocol: Request.Scheme)!;

        // 3) message
        var html = $@"
<h2>Підтвердження пошти</h2>
<p>Натисни кнопку/посилання, щоб підтвердити email:</p>
<p><a href=""{link}"">Confirm Email</a></p>
<p>Якщо ти не реєструвався — просто ігноруй цей лист.</p>";

        // 4) send email
        
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            ModelState.AddModelError("", "Email не задано. Перевір форму реєстрації.");
            return View(vm);
        }

        await _emailSender.SendAsync(user.Email, "Lihtar ArtPub - Confirm your email", html);


        // НЕ логінимо, доки не підтвердить
        return RedirectToAction(nameof(RegistrationSuccess));
    }

    [HttpGet]
    public IActionResult RegistrationSuccess()
        => View();

    // -------------------- CONFIRM EMAIL --------------------

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            TempData["Error"] = "Посилання недійсне або протерміноване.";
            return RedirectToAction(nameof(Login));
        }

        return View();
    }

    // -------------------- LOGOUT --------------------

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
