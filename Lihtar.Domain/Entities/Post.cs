namespace Lihtar.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? ImageUrl { get; set; }

    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; } = false;
}
