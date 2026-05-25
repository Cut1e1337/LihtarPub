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

    public TableBoardController(
        ITableService tableService,
        IReservationService reservationService)
    {
        _tableService = tableService;
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tables = await _tableService.GetAllAsync();
        var reservations = await _reservationService.GetAllAsync();

        var today = DateTime.Today;
        var now = DateTime.Now.TimeOfDay;

        var result = tables
            .OrderBy(x => x.TableNumber)
            .Select(table =>
            {
                var activeReservation = reservations
                    .Where(r =>
                        r.TableId == table.Id &&
                        r.ReservationDate.Date == today &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.Status != ReservationStatus.Completed &&
                        r.EndTime >= now)
                    .OrderBy(r => r.StartTime)
                    .FirstOrDefault();

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
                }
                else if (activeReservation != null)
                {
                    vm.Status = activeReservation.Status == ReservationStatus.Confirmed
                        ? "Reserved"
                        : "Pending";

                    vm.ReservationId = activeReservation.Id;
                    vm.ReservationDate = activeReservation.ReservationDate;
                    vm.StartTime = activeReservation.StartTime;
                    vm.EndTime = activeReservation.EndTime;
                    vm.Comment = activeReservation.Comment;
                }
                else
                {
                    vm.Status = "Free";
                }

                return vm;
            })
            .ToList();

        return View(result);
    }
}