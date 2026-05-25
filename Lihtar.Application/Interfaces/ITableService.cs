using Lihtar.Application.DTOs;

namespace Lihtar.Application.Interfaces;

public interface ITableService
{
    Task<List<TableDto>> GetAllAsync();

    Task<TableDto?> GetByIdAsync(Guid id);

    Task CreateAsync(TableDto dto);

    Task UpdateAsync(TableDto dto);

    Task DeleteAsync(Guid id);
}