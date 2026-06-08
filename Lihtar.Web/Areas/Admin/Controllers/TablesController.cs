using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class TablesController : AdminBaseController
{
    private readonly ITableService _service;

    public TablesController(ITableService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tables = await _service.GetAllAsync();
        return View(tables);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TableDto
        {
            TableNumber = 1,
            Seats = 2,
            IsVip = false,
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TableNumber,Seats,IsVip,IsActive")] TableDto dto)
    {
        if (dto.TableNumber <= 0)
            ModelState.AddModelError(nameof(dto.TableNumber), "Номер столика має бути більше 0.");

        if (dto.Seats <= 0)
            ModelState.AddModelError(nameof(dto.Seats), "Кількість місць має бути більше 0.");

        var tables = await _service.GetAllAsync();

        var tableExists = tables.Any(x => x.TableNumber == dto.TableNumber);

        if (tableExists)
            ModelState.AddModelError(nameof(dto.TableNumber), "Столик з таким номером вже існує.");

        if (!ModelState.IsValid)
            return View(dto);

        await _service.CreateAsync(dto);

        TempData["Success"] = "Столик успішно додано.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var tables = await _service.GetAllAsync();

        var table = tables.FirstOrDefault(x => x.Id == id);

        if (table == null)
            return NotFound();

        return View(table);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TableDto dto)
    {
        if (dto.Id == Guid.Empty)
            return BadRequest();

        dto.IsVip = Request.Form["IsVip"].Contains("true");
        dto.IsActive = Request.Form["IsActive"].Contains("true");

        if (dto.TableNumber <= 0)
            ModelState.AddModelError(nameof(dto.TableNumber), "Номер столика має бути більше 0.");

        if (dto.Seats <= 0)
            ModelState.AddModelError(nameof(dto.Seats), "Кількість місць має бути більше 0.");

        var tables = await _service.GetAllAsync();

        var tableExists = tables.Any(x =>
            x.TableNumber == dto.TableNumber &&
            x.Id != dto.Id);

        if (tableExists)
            ModelState.AddModelError(nameof(dto.TableNumber), "Столик з таким номером вже існує.");

        if (!ModelState.IsValid)
            return View(dto);

        await _service.UpdateAsync(dto);

        TempData["Success"] = "Столик успішно оновлено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var table = await _service.GetByIdAsync(id);

        if (table == null)
            return NotFound();

        return View(table);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _service.DeleteAsync(id);

        TempData["Success"] = "Столик успішно видалено.";
        return RedirectToAction(nameof(Index));
    }
}