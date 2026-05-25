namespace Lihtar.Domain.Entities;

public class PaymentItem
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public Guid OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TotalPrice => Quantity * Price;
}