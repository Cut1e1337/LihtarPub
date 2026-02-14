namespace Lihtar.Application.DTOs;

public class EventDto
{
    public Guid Id { get; set; }

    public Guid EventCategoryId { get; set; }
    public string CategoryName { get; set; } = "";

    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }
    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}
