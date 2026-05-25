namespace Lihtar.Web.ViewModels.Payments;

public class SelectTablePaymentVm
{
    public Guid TableId { get; set; }

    public int TableNumber { get; set; }

    public int Seats { get; set; }

    public bool IsVip { get; set; }

    public Guid? ActiveOrderId { get; set; }

    public decimal? TotalPrice { get; set; }
}