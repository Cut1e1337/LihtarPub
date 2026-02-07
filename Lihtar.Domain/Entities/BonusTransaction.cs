namespace Lihtar.Domain.Entities;

public class BonusTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public int Points { get; set; } // + або -
    public string Reason { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
