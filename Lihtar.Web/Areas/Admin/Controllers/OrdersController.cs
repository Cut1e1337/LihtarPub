using Lihtar.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lihtar.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class OrdersController : AdminBaseController
{
    private readonly IOrderService _orderService;
    private readonly IMenuItemService _menuItemService;

    public OrdersController(
        IOrderService orderService,
        IMenuItemService menuItemService)
    {
        _orderService = orderService;
        _menuItemService = menuItemService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenForTable(Guid tableId)
    {
        var orderId = await _orderService.OpenForTableAsync(tableId);

        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenFromReservation(Guid reservationId)
    {
        var orderId = await _orderService.OpenFromReservationAsync(reservationId);

        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);

        if (order == null)
            return NotFound();

        var menuItems = await _menuItemService.GetAllAsync();

        ViewBag.MenuItems = menuItems
            .Where(x => x.IsAvailable)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.Name} — {x.Price:0.00} грн"
            })
            .ToList();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(Guid orderId, Guid menuItemId, int quantity)
    {
        await _orderService.AddItemAsync(orderId, menuItemId, quantity);

        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItem(Guid orderId, Guid orderItemId, int quantity)
    {
        await _orderService.UpdateItemQuantityAsync(orderId, orderItemId, quantity);

        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid orderItemId)
    {
        await _orderService.RemoveItemAsync(orderId, orderItemId);

        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid orderId)
    {
        await _orderService.CompleteAsync(orderId);

        return RedirectToAction("Index", "TableBoard", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        await _orderService.CancelAsync(orderId);

        return RedirectToAction("Index", "TableBoard", new { area = "Admin" });
    }
}