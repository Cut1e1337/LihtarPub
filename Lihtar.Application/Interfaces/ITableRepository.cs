using Lihtar.Domain.Entities;

namespace Lihtar.Application.Interfaces;

public interface ITableRepository
{
    Task<List<Table>> GetAllAsync();

    Task<Table?> GetByIdAsync(Guid id);

    Task AddAsync(Table table);

    void Remove(Table table);

    Task SaveChangesAsync();
}