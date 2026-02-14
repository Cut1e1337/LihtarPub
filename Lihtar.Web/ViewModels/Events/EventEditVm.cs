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
    public int AvailableSeats { get; set; } // для показу, у Create можна ігнорувати

    public bool IsActive { get; set; } = true;

    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
}
