namespace Lihtar.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid EventCategoryId { get; set; }
    public EventCategory? EventCategory { get; set; }

    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }
    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<EventTicket> Tickets { get; set; } = new List<EventTicket>();
    public ICollection<EventReview> EventReviews { get; set; } = new List<EventReview>();
}
