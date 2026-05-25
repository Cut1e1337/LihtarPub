using Lihtar.Domain.Enums;

namespace Lihtar.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? TableId { get; set; }
    public Table? Table { get; set; }

    public Guid? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public decimal TotalPrice { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}