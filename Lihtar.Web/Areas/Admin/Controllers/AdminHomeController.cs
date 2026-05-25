using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminHomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction(
            "Index",
            "TableBoard",
            new { area = "Admin" }
        );
    }
}