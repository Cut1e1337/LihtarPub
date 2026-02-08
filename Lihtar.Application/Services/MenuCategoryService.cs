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
        var list = await _repo.GetAllAsync();

        return list
            .Select(x => new MenuCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToList();
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

    public async Task<Guid> CreateAsync(MenuCategoryDto dto)
    {
        var entity = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(MenuCategoryDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id)
                     ?? throw new InvalidOperationException("MenuCategory not found");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.SortOrder = dto.SortOrder;
        entity.IsActive = dto.IsActive;

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id)
                     ?? throw new InvalidOperationException("MenuCategory not found");

        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }
}
