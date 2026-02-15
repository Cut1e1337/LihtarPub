namespace Lihtar.Web.ViewModels.PublicEvents;

public class PublicEventDetailsVm
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string CategoryName { get; set; } = "";

    public DateTime EventDate { get; set; }
    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }

    public bool CanBook { get; set; }
}
