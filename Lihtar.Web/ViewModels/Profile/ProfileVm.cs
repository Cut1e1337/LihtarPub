using System.ComponentModel.DataAnnotations;

namespace Lihtar.Web.ViewModels.Profile;

public class ProfileVm
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введіть ім’я")]
    [Display(Name = "Ім’я")]
    public string FullName { get; set; } = "";

    [Display(Name = "Дата народження")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    public string? AvatarUrl { get; set; }

    public int BonusPoints { get; set; }

    [Display(Name = "Новий аватар")]
    public IFormFile? AvatarFile { get; set; }
}