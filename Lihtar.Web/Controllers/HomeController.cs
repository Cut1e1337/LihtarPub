using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
