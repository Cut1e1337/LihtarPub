using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class TableRepository : ITableRepository
{
    private readonly ArtPubDbContext _db;

    public TableRepository(ArtPubDbContext db)
    {
        _db = db;
    }

    public async Task<List<Table>> GetAllAsync()
    {
        return await _db.Tables
            .AsNoTracking()
            .OrderBy(x => x.TableNumber)
            .ToListAsync();
    }

    public async Task<Table?> GetByIdAsync(Guid id)
    {
        return await _db.Tables
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Table table)
    {
        await _db.Tables.AddAsync(table);
    }

    public void Remove(Table table)
    {
        _db.Tables.Remove(table);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}