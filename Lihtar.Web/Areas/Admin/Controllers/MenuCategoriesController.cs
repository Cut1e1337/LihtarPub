using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lihtar.Web.Areas.Admin.Controllers;

public class MenuCategoriesController : AdminBaseController
{
    private readonly IMenuCategoryService _service;

    public MenuCategoriesController(IMenuCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _service.GetAllAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create() => View(new MenuCategoryDto { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuCategoryDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _service.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    // ✅ ВАЖЛИВО: GET Edit
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound();

        return View(dto);
    }

    // ✅ POST Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MenuCategoryDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _service.UpdateAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null) return NotFound();

        return View(dto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
