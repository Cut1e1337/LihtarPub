namespace Lihtar.Domain.Entities;

public class EventReview
{
    public Guid ReviewId { get; set; }
    public Review? Review { get; set; }

    public Guid EventId { get; set; }
    public Event? Event { get; set; }
}
