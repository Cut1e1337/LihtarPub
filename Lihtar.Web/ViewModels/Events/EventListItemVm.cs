namespace Lihtar.Web.ViewModels.Events;

public class EventListItemVm
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime EventDate { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
