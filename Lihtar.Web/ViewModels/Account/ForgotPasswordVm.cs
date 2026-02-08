using System.ComponentModel.DataAnnotations;

namespace Lihtar.Web.ViewModels.Account;

public class ForgotPasswordVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;
}
