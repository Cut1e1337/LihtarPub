using System.ComponentModel.DataAnnotations;

namespace Lihtar.Web.ViewModels.Events;

public class EventCategoryEditVm
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }
}
