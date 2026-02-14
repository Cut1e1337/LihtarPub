using Lihtar.Application.DTOs;

namespace Lihtar.Application.Interfaces;

public interface IMenuItemService
{
    Task<List<MenuItemDto>> GetAllAsync();
    Task<MenuItemDto?> GetByIdAsync(Guid id);
    Task CreateAsync(MenuItemDto dto);
    Task UpdateAsync(MenuItemDto dto);
    Task DeleteAsync(Guid id);
}
