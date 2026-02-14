using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class EventCategoryRepository : IEventCategoryRepository
{
    private readonly ArtPubDbContext _db;
    public EventCategoryRepository(ArtPubDbContext db) => _db = db;

    public Task<List<EventCategory>> GetAllAsync()
        => _db.EventCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    public Task<EventCategory?> GetByIdAsync(Guid id)
        => _db.EventCategories.FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(EventCategory entity)
        => await _db.EventCategories.AddAsync(entity);

    public void Remove(EventCategory entity)
        => _db.EventCategories.Remove(entity);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

public class EventRepository : IEventRepository
{
    private readonly ArtPubDbContext _db;
    public EventRepository(ArtPubDbContext db) => _db = db;

    public Task<List<Event>> GetAllAsync()
        => _db.Events
            .Include(x => x.EventCategory)
            .AsNoTracking()
            .OrderByDescending(x => x.EventDate)
            .ToListAsync();

    public Task<Event?> GetByIdAsync(Guid id)
        => _db.Events
            .Include(x => x.EventCategory)
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(Event entity)
        => await _db.Events.AddAsync(entity);

    public void Remove(Event entity)
        => _db.Events.Remove(entity);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
