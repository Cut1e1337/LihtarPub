namespace Lihtar.Application.DTOs;

public class EventCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
