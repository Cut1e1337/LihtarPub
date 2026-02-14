using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class MenuItemTagRepository : IMenuItemTagRepository
{
    private readonly ArtPubDbContext _db;
    public MenuItemTagRepository(ArtPubDbContext db) => _db = db;

    public Task<List<MenuItemTag>> GetAllAsync()
        => _db.MenuItemTags.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    public Task<MenuItemTag?> GetByIdAsync(Guid id)
        => _db.MenuItemTags.FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(MenuItemTag tag)
    {
        _db.MenuItemTags.Add(tag);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuItemTag tag)
    {
        _db.MenuItemTags.Update(tag);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.MenuItemTags.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return;
        _db.MenuItemTags.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
