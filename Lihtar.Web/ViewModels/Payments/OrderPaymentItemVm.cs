namespace Lihtar.Web.ViewModels.Payments;

public class OrderPaymentItemVm
{
    public Guid OrderItemId { get; set; }

    public string MenuItemName { get; set; } = "";

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TotalPrice { get; set; }
}