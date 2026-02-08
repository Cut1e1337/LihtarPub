using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Application.Services;

public class MenuCategoryService : IMenuCategoryService
{
    private readonly ArtPubDbContext _db;

    public MenuCategoryService(ArtPubDbContext db)
    {
        _db = db;
    }

    public async Task<List<MenuCategoryDto>> GetAllAsync()
    {
        return await _db.MenuCategories
            .OrderBy(x => x.SortOrder)
            .Select(x => new MenuCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<MenuCategoryDto?> GetByIdAsync(Guid id)
    {
        return await _db.MenuCategories
            .Where(x => x.Id == id)
            .Select(x => new MenuCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
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

        _db.MenuCategories.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(MenuCategoryDto dto)
    {
        var entity = await _db.MenuCategories.FirstOrDefaultAsync(x => x.Id == dto.Id)
                     ?? throw new InvalidOperationException("MenuCategory not found");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.SortOrder = dto.SortOrder;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.MenuCategories.FirstOrDefaultAsync(x => x.Id == id)
                     ?? throw new InvalidOperationException("MenuCategory not found");

        _db.MenuCategories.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
