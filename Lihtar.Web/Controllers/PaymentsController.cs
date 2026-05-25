using Lihtar.Application.Interfaces;
using Lihtar.Infrastructure.Identity;
using Lihtar.Web.ViewModels.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly ITableService _tableService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentsController(
        IOrderService orderService,
        IPaymentService paymentService,
        ITableService tableService,
        UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _tableService = tableService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> SelectTable()
    {
        var tables = await _tableService.GetAllAsync();

        var vm = new List<SelectTablePaymentVm>();

        foreach (var table in tables.Where(x => x.IsActive).OrderBy(x => x.TableNumber))
        {
            var activeOrder = await _orderService.GetActiveByTableIdAsync(table.Id);

            decimal? remainingAmount = null;

            if (activeOrder != null)
            {
                remainingAmount = await _paymentService.GetRemainingAmountByOrderAsync(activeOrder.Id);
            }

            vm.Add(new SelectTablePaymentVm
            {
                TableId = table.Id,
                TableNumber = table.TableNumber,
                Seats = table.Seats,
                IsVip = table.IsVip,
                ActiveOrderId = activeOrder?.Id,
                TotalPrice = remainingAmount
            });
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Order(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);

        if (order == null)
            return NotFound();

        var paidQuantities = await _paymentService.GetPaidQuantitiesByOrderAsync(order.Id);

        var visibleItems = order.Items
            .Select(x =>
            {
                var paidQty = paidQuantities.ContainsKey(x.Id)
                    ? paidQuantities[x.Id]
                    : 0;

                var remainingQty = x.Quantity - paidQty;

                return new OrderPaymentItemVm
                {
                    OrderItemId = x.Id,
                    MenuItemName = x.MenuItemName,
                    Quantity = remainingQty,
                    Price = x.Price,
                    TotalPrice = remainingQty * x.Price
                };
            })
            .Where(x => x.Quantity > 0)
            .ToList();

        var vm = new OrderPaymentVm
        {
            OrderId = order.Id,
            TableNumber = order.TableNumber,
            TotalPrice = visibleItems.Sum(x => x.TotalPrice),
            Items = visibleItems
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayFull(Guid orderId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var paymentId = await _paymentService.CreateFullPaymentAsync(orderId, user.Id);

        return RedirectToAction(nameof(Confirm), new { id = paymentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PaySplit(Guid orderId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Account");

        var selectedItems = new Dictionary<Guid, int>();

        foreach (var key in Request.Form.Keys)
        {
            if (!key.StartsWith("qty_"))
                continue;

            var idText = key.Replace("qty_", "");

            if (!Guid.TryParse(idText, out var orderItemId))
                continue;

            if (!int.TryParse(Request.Form[key], out var quantity))
                continue;

            if (quantity > 0)
                selectedItems[orderItemId] = quantity;
        }

        try
        {
            var paymentId = await _paymentService.CreateSplitPaymentAsync(orderId, user.Id, selectedItems);
            return RedirectToAction(nameof(Confirm), new { id = paymentId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Order), new { id = orderId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id);

        if (payment == null)
            return NotFound();

        return View(payment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid paymentId, string cardNumber, bool useBonuses)
    {
        await _paymentService.ConfirmMockPaymentAsync(paymentId, cardNumber, useBonuses);

        return RedirectToAction(nameof(Success), new { id = paymentId });
    }

    [HttpGet]
    public async Task<IActionResult> Success(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id);

        if (payment == null)
            return NotFound();

        return View(payment);
    }
}