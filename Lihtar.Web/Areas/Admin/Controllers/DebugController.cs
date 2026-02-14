using Lihtar.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Web.Areas.Admin.Controllers;

public class DebugController : AdminBaseController
{
    private readonly ArtPubDbContext _db;
    private readonly IConfiguration _cfg;

    public DebugController(ArtPubDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    [HttpGet]
    public async Task<IActionResult> Categories()
    {
        var cs = _cfg.GetConnectionString("DefaultConnection") ?? "NULL";
        var dbName = _db.Database.GetDbConnection().Database;

        var count = await _db.MenuCategories.CountAsync();
        var list = await _db.MenuCategories
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();

        return Json(new { connectionString = cs, database = dbName, count, list });
    }
}
