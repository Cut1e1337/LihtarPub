using Lihtar.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class ReservationsController : AdminBaseController
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var reservations = await _reservationService.GetAllAsync();
        return View(reservations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id)
    {
        await _reservationService.ConfirmAsync(id);
        TempData["Success"] = "Бронювання підтверджено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _reservationService.CancelAsync(id);
        TempData["Success"] = "Бронювання скасовано.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id)
    {
        await _reservationService.CompleteAsync(id);
        TempData["Success"] = "Бронювання завершено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _reservationService.DeleteAsync(id);
        TempData["Success"] = "Бронювання видалено.";
        return RedirectToAction(nameof(Index));
    }
}