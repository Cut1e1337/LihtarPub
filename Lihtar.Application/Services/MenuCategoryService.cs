using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Services;

public class MenuCategoryService : IMenuCategoryService
{
    private readonly IMenuCategoryRepository _repo;

    public MenuCategoryService(IMenuCategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<MenuCategoryDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();

        return items.Select(x => new MenuCategoryDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive
        }).ToList();
    }

    public async Task<MenuCategoryDto?> GetByIdAsync(Guid id)
    {
        var x = await _repo.GetByIdAsync(id);
        if (x is null) return null;

        return new MenuCategoryDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive
        };
    }

    public async Task CreateAsync(MenuCategoryDto dto)
    {
        // мінімальна валідація
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");

        var entity = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync(); // ✅ ВАЖЛИВО
    }

    public async Task UpdateAsync(MenuCategoryDto dto)
    {
        if (dto.Id == Guid.Empty)
            throw new ArgumentException("Id is required");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");

        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity is null)
            throw new InvalidOperationException("MenuCategory not found");

        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.SortOrder = dto.SortOrder;
        entity.IsActive = dto.IsActive;

        await _repo.SaveChangesAsync(); // ✅ ВАЖЛИВО
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return;

        _repo.Remove(entity);
        await _repo.SaveChangesAsync(); // ✅ ВАЖЛИВО
    }
}
