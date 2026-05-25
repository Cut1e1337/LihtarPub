namespace Lihtar.Application.DTOs;

public class OrderItemDto
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid MenuItemId { get; set; }

    public string MenuItemName { get; set; } = "";

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TotalPrice { get; set; }
}