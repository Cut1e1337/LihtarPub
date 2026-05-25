namespace Lihtar.Web.ViewModels.Payments;

public class OrderPaymentVm
{
    public Guid OrderId { get; set; }

    public int? TableNumber { get; set; }

    public decimal TotalPrice { get; set; }

    public List<OrderPaymentItemVm> Items { get; set; } = new();
}