using System.ComponentModel.DataAnnotations;

namespace Lihtar.Web.ViewModels.Account;

public class ResetPasswordVm
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = default!;

    [Required, DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = default!;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = default!;
}
