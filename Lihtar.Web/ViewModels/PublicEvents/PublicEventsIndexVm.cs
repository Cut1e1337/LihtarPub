namespace Lihtar.Web.ViewModels.PublicEvents;

public class PublicEventsIndexVm
{
    public string? Q { get; set; }
    public Guid? CategoryId { get; set; }
    public bool OnlyUpcoming { get; set; } = true;
    public bool OnlyActive { get; set; } = true;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalPages { get; set; }

    public List<PublicEventCategoryFilterVm> Categories { get; set; } = new();
    public List<PublicEventCardVm> Items { get; set; } = new();
}
