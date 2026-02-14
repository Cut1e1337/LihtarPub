namespace Lihtar.Web.Areas.Admin.ViewModels.Users;

public class UsersListVm
{
    public string? Q { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }

    public List<UserRowVm> Users { get; set; } = new();

    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}
