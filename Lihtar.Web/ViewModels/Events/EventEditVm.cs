using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lihtar.Web.ViewModels.Events;

public class EventEditVm
{
    public Guid? Id { get; set; }

    public Guid EventCategoryId { get; set; }

    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public DateTime EventDate { get; set; } = DateTime.Now.AddDays(1);
    public int DurationMinutes { get; set; } = 60;

    public decimal Price { get; set; }
    public int TotalSeats { get; set; } = 20;
    public int AvailableSeats { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }

    // ✅ ВАЖЛИВО: категорії тепер тут, а не у ViewBag
    public List<SelectListItem> Categories { get; set; } = new();
}
