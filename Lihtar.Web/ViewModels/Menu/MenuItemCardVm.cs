namespace Lihtar.Web.ViewModels.Menu
{

    public class MenuItemCardVm
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? Calories { get; set; }
        public int? WeightGrams { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = "";

        public List<string> Tags { get; set; } = new();
    }
}
