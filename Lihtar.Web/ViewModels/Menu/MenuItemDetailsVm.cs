namespace Lihtar.Web.ViewModels.Menu;

public class MenuItemDetailsVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? Calories { get; set; }
    public int? WeightGrams { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }

    public string CategoryName { get; set; } = "";
    public List<string> Tags { get; set; } = new();

    public bool IsFavorite { get; set; }
    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }

    public List<MenuReviewVm> Reviews { get; set; } = new();
}

public class MenuReviewVm
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string UserEmail { get; set; } = "";
    public string UserFullName { get; set; } = "";
    public string? UserAvatarUrl { get; set; }

    public int Rating { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsMine { get; set; }
}