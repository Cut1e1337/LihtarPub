namespace Lihtar.Application.DTOs;

public class PaymentItemDto
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    public Guid OrderItemId { get; set; }

    public string MenuItemName { get; set; } = "";

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TotalPrice { get; set; }
}