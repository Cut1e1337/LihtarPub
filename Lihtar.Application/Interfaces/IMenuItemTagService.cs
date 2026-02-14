using Lihtar.Application.DTOs;

namespace Lihtar.Application.Interfaces;

public interface IMenuItemTagService
{
    Task<List<MenuItemTagDto>> GetAllAsync();
    Task<MenuItemTagDto?> GetByIdAsync(Guid id);
    Task CreateAsync(MenuItemTagDto dto);
    Task UpdateAsync(MenuItemTagDto dto);
    Task DeleteAsync(Guid id);
}
