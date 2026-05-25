using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Lihtar.Web.ViewModels.Reservations;

public class CreateReservationVm
{
    [Required]
    [Display(Name = "Столик")]
    public Guid TableId { get; set; }

    [Required]
    [Display(Name = "Дата бронювання")]
    [DataType(DataType.Date)]
    public DateTime ReservationDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Початок")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; } = new TimeSpan(18, 0, 0);

    [Required]
    [Display(Name = "Кінець")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; } = new TimeSpan(20, 0, 0);

    [Display(Name = "Коментар")]
    public string? Comment { get; set; }

    public List<SelectListItem> Tables { get; set; } = new();
}