using System.Text;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Enums;
using Lihtar.Infrastructure.Identity;
using Lihtar.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

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

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

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
            EmailConfirmed = false,
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

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var link = Url.Action(
            nameof(ConfirmEmail),
            "Account",
            new { userId = user.Id, token },
            protocol: Request.Scheme)!;

        var html = $@"
<h2>Підтвердження пошти</h2>
<p>Натисни кнопку/посилання, щоб підтвердити email:</p>
<p><a href=""{link}"">Confirm Email</a></p>
<p>Якщо ти не реєструвався — просто ігноруй цей лист.</p>";

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            ModelState.AddModelError("", "Email не задано. Перевір форму реєстрації.");
            return View(vm);
        }

        await _emailSender.SendAsync(user.Email, "Lihtar ArtPub - Confirm your email", html);

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

    // -------------------- FORGOT PASSWORD --------------------

    [HttpGet]
    public IActionResult ForgotPassword()
        => View(new ForgotPasswordVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);

        // не палимо існування email
        if (user is null || user.IsBlocked || !await _userManager.IsEmailConfirmedAsync(user))
            return RedirectToAction(nameof(ForgotPasswordConfirmation));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // ✅ token -> URL-safe
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var tokenEncoded = WebEncoders.Base64UrlEncode(tokenBytes);

        var link = Url.Action(
            nameof(ResetPassword),
            "Account",
            new { userId = user.Id, token = tokenEncoded },
            protocol: Request.Scheme)!;

        var html = $@"
<h2>Скидання паролю</h2>
<p>Натисни посилання, щоб встановити новий пароль:</p>
<p><a href=""{link}"">Reset password</a></p>
<p>Якщо ти не робив запит — просто ігноруй цей лист.</p>";

        await _emailSender.SendAsync(user.Email!, "Lihtar ArtPub - Reset password", html);

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
        => View();

    // -------------------- RESET PASSWORD --------------------

    [HttpGet]
    public IActionResult ResetPassword(Guid userId, string token)
    {
        // ✅ ВАЖЛИВО: тут НЕ декодуємо
        // кладемо tokenEncoded у VM і передаємо в hidden input
        return View(new ResetPasswordVm
        {
            UserId = userId,
            Token = token
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByIdAsync(vm.UserId.ToString());
        if (user is null)
        {
            ModelState.AddModelError("", "Користувача не знайдено");
            return View(vm);
        }

        if (user.IsBlocked)
        {
            ModelState.AddModelError("", "Акаунт заблоковано");
            return View(vm);
        }

        if (string.IsNullOrWhiteSpace(vm.Token))
        {
            ModelState.AddModelError("", "Token порожній. Спробуй ще раз отримати лист для скидання паролю.");
            return View(vm);
        }

        // ✅ tokenEncoded -> token (оригінальний)
        string tokenDecoded;
        try
        {
            var tokenBytes = WebEncoders.Base64UrlDecode(vm.Token);
            tokenDecoded = Encoding.UTF8.GetString(tokenBytes);
        }
        catch
        {
            ModelState.AddModelError("", "Token пошкоджено. Спробуй ще раз отримати лист для скидання паролю.");
            return View(vm);
        }

        var result = await _userManager.ResetPasswordAsync(user, tokenDecoded, vm.Password);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", $"{e.Code}: {e.Description}");

            return View(vm);
        }

        return RedirectToAction(nameof(ResetPasswordSuccess));
    }

    [HttpGet]
    public IActionResult ResetPasswordSuccess()
        => View();

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
