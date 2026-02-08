using Lihtar.Domain.Entities;

namespace Lihtar.Application.Interfaces;

public interface IMenuCategoryRepository
{
    Task<List<MenuCategory>> GetAllAsync();
    Task<MenuCategory?> GetByIdAsync(Guid id);

    Task AddAsync(MenuCategory entity);
    void Remove(MenuCategory entity);

    Task SaveChangesAsync();
}
