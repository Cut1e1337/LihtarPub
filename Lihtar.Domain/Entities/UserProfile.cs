namespace Lihtar.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string? AvatarUrl { get; set; }
    public DateTime? BirthDate { get; set; }
    public int BonusPoints { get; set; }
}
