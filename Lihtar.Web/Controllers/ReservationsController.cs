using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Infrastructure.Identity;
using Lihtar.Web.ViewModels.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lihtar.Web.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly ITableService _tableService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReservationsController(
        IReservationService reservationService,
        ITableService tableService,
        UserManager<ApplicationUser> userManager)
    {
        _reservationService = reservationService;
        _tableService = tableService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new CreateReservationVm();
        await FillTablesAsync(vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReservationVm vm)
    {
        await FillTablesAsync(vm);

        if (!ModelState.IsValid)
            return View(vm);

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        try
        {
            var dto = new ReservationDto
            {
                UserId = user.Id,
                TableId = vm.TableId,
                ReservationDate = vm.ReservationDate,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                Comment = vm.Comment
            };

            await _reservationService.CreateAsync(dto);

            TempData["Success"] = "Бронювання створено. Очікуйте підтвердження адміністратора.";

            return RedirectToAction(nameof(My));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> My()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var reservations = await _reservationService.GetByUserIdAsync(user.Id);

        return View(reservations);
    }

    private async Task FillTablesAsync(CreateReservationVm vm)
    {
        var tables = await _tableService.GetAllAsync();

        vm.Tables = tables
            .Where(x => x.IsActive)
            .OrderBy(x => x.TableNumber)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"Столик №{x.TableNumber} — {x.Seats} місць" + (x.IsVip ? " — VIP" : "")
            })
            .ToList();
    }
}