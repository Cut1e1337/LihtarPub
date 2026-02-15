using System.ComponentModel.DataAnnotations;

namespace Lihtar.Web.ViewModels.Menu
{
    public class MenuIndexVm
    {
        public string? Q { get; set; }

        public Guid? CategoryId { get; set; }

        [Display(Name = "Only available")]
        public bool OnlyAvailable { get; set; } = true;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public int Total { get; set; }
        public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

        public List<MenuCategoryFilterVm> Categories { get; set; } = new();
        public List<MenuItemCardVm> Items { get; set; } = new();
    }
}
