namespace Lihtar.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public int Rating { get; set; } // 1-5
    public string Text { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; } = false;

    public ICollection<MenuItemReview> MenuItemReviews { get; set; } = new List<MenuItemReview>();
    public ICollection<EventReview> EventReviews { get; set; } = new List<EventReview>();
}
