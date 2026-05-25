using Lihtar.Application.Interfaces;
using Lihtar.Domain.Enums;
using Lihtar.Web.Areas.Admin.ViewModels.TableBoard;
using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class TableBoardController : AdminBaseController
{
    private readonly ITableService _tableService;
    private readonly IReservationService _reservationService;
    private readonly IOrderService _orderService;

    public TableBoardController(
        ITableService tableService,
        IReservationService reservationService,
        IOrderService orderService)
    {
        _tableService = tableService;
        _reservationService = reservationService;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tables = await _tableService.GetAllAsync();
        var reservations = await _reservationService.GetAllAsync();

        var today = DateTime.Today;
        var now = DateTime.Now.TimeOfDay;
        var reserveWindow = TimeSpan.FromMinutes(90);

        var result = new List<TableBoardItemVm>();

        foreach (var table in tables.OrderBy(x => x.TableNumber))
        {
            var vm = new TableBoardItemVm
            {
                TableId = table.Id,
                TableNumber = table.TableNumber,
                Seats = table.Seats,
                IsVip = table.IsVip,
                IsActive = table.IsActive
            };

            if (!table.IsActive)
            {
                vm.Status = "Inactive";
                result.Add(vm);
                continue;
            }

            var activeOrder = await _orderService.GetActiveByTableIdAsync(table.Id);

            if (activeOrder != null)
            {
                vm.Status = "Busy";
                vm.ActiveOrderId = activeOrder.Id;
                result.Add(vm);
                continue;
            }

            var reservationSoon = reservations
                .Where(r =>
                    r.TableId == table.Id &&
                    r.ReservationDate.Date == today &&
                    r.Status == ReservationStatus.Confirmed &&
                    r.StartTime >= now &&
                    r.StartTime <= now.Add(reserveWindow))
                .OrderBy(r => r.StartTime)
                .FirstOrDefault();

            if (reservationSoon != null)
            {
                vm.Status = "Reserved";
                vm.ReservationId = reservationSoon.Id;
                vm.ReservationDate = reservationSoon.ReservationDate;
                vm.StartTime = reservationSoon.StartTime;
                vm.EndTime = reservationSoon.EndTime;
                vm.Comment = reservationSoon.Comment;

                result.Add(vm);
                continue;
            }

            vm.Status = "Free";
            result.Add(vm);
        }

        return View(result);
    }
}