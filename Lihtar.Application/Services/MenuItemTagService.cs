using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Services;

public class MenuItemTagService : IMenuItemTagService
{
    private readonly IMenuItemTagRepository _repo;

    public MenuItemTagService(IMenuItemTagRepository repo) => _repo = repo;

    public async Task<List<MenuItemTagDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(x => new MenuItemTagDto { Id = x.Id, Name = x.Name }).ToList();

    public async Task<MenuItemTagDto?> GetByIdAsync(Guid id)
    {
        var tag = await _repo.GetByIdAsync(id);
        return tag is null ? null : new MenuItemTagDto { Id = tag.Id, Name = tag.Name };
    }

    public async Task CreateAsync(MenuItemTagDto dto)
        => await _repo.AddAsync(new MenuItemTag { Id = Guid.NewGuid(), Name = dto.Name });

    public async Task UpdateAsync(MenuItemTagDto dto)
    {
        var tag = await _repo.GetByIdAsync(dto.Id);
        if (tag is null) return;
        tag.Name = dto.Name;
        await _repo.UpdateAsync(tag);
    }

    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
}
