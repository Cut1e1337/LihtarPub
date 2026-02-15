namespace Lihtar.Web.ViewModels.PublicEvents;

public class PublicEventCardVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public string? ImageUrl { get; set; }
}
