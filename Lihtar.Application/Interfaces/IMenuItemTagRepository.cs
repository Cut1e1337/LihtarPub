using Lihtar.Domain.Entities;

namespace Lihtar.Application.Interfaces;

public interface IMenuItemTagRepository
{
    Task<List<MenuItemTag>> GetAllAsync();
    Task<MenuItemTag?> GetByIdAsync(Guid id);
    Task AddAsync(MenuItemTag tag);
    Task UpdateAsync(MenuItemTag tag);
    Task DeleteAsync(Guid id);
}
