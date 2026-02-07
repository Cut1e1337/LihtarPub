namespace Lihtar.Domain.Entities;

public class EventCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
