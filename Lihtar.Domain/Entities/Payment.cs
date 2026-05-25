using Lihtar.Domain.Enums;

namespace Lihtar.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid UserId { get; set; }

    public decimal Amount { get; set; }

    public PaymentType Type { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    public string? CardMask { get; set; }

    public ICollection<PaymentItem> Items { get; set; } = new List<PaymentItem>();
}