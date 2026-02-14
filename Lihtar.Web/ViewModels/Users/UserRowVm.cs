namespace Lihtar.Web.Areas.Admin.ViewModels.Users;

public class UserRowVm
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public bool IsBlocked { get; set; }
    public string Roles { get; set; } = "";
}
