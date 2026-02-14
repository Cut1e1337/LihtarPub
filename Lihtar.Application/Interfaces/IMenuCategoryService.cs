using Lihtar.Application.DTOs;

namespace Lihtar.Application.Interfaces;

public interface IMenuCategoryService
{
    Task<List<MenuCategoryDto>> GetAllAsync();
    Task<MenuCategoryDto?> GetByIdAsync(Guid id);

    Task CreateAsync(MenuCategoryDto dto);
    Task UpdateAsync(MenuCategoryDto dto);
    Task DeleteAsync(Guid id);
}
