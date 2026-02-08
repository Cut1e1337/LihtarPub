using System.ComponentModel.DataAnnotations;

namespace Lihtar.Web.ViewModels.Account;

public class RegisterVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string FullName { get; set; } = default!;

    [Required, DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = default!;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = default!;
}
