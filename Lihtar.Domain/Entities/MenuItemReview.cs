namespace Lihtar.Domain.Entities;

public class MenuItemReview
{
    public Guid ReviewId { get; set; }
    public Review? Review { get; set; }

    public Guid MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }
}
