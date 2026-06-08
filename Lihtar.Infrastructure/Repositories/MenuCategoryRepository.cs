using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class MenuCategoryRepository : IMenuCategoryRepository
{
    private readonly ArtPubDbContext _db;

    public MenuCategoryRepository(ArtPubDbContext db)
    {
        _db = db;
    }

    public Task<List<MenuCategory>> GetAllAsync()
        => _db.MenuCategories
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

    public Task<MenuCategory?> GetByIdAsync(Guid id)
        => _db.MenuCategories
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

    public async Task AddAsync(MenuCategory entity)
        => await _db.MenuCategories.AddAsync(entity);

    public void Remove(MenuCategory entity)
    {
        entity.IsDeleted = true;
        _db.MenuCategories.Update(entity);
    }

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();
}