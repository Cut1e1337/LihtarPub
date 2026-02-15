namespace Lihtar.Web.ViewModels.Menu
{
    public class MenuCategoryFilterVm
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public bool IsActive { get; set; }
    }
}
