using Lihtar.Domain.Enums;

namespace Lihtar.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public decimal Amount { get; set; }

    public PaymentType Type { get; set; }

    public PaymentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? CardMask { get; set; }

    public List<PaymentItemDto> Items { get; set; } = new();
}