using Lihtar.Domain.Enums;

namespace Lihtar.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? TableId { get; set; }
    public int? TableNumber { get; set; }

    public Guid? ReservationId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalPrice { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}