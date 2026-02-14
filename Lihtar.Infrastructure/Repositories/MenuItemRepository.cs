using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly ArtPubDbContext _db;
    public MenuItemRepository(ArtPubDbContext db) => _db = db;

    public Task<List<MenuItem>> GetAllAsync()
        => _db.MenuItems
            .Include(x => x.MenuCategory)
            .Include(x => x.TagLinks).ThenInclude(t => t.Tag)
            .AsNoTracking()
            .ToListAsync();

    public Task<MenuItem?> GetByIdAsync(Guid id)
        => _db.MenuItems
            .Include(x => x.TagLinks)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(MenuItem item)
    {
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuItem item)
    {
        _db.MenuItems.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return;
        _db.MenuItems.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
