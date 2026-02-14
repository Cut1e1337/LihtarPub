using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repo;

    public MenuItemService(IMenuItemRepository repo) => _repo = repo;

    public async Task<List<MenuItemDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(MapToDto).ToList();

    public async Task<MenuItemDto?> GetByIdAsync(Guid id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? null : MapToDto(item);
    }

    public async Task CreateAsync(MenuItemDto dto)
    {
        var entity = new MenuItem
        {
            Id = Guid.NewGuid(),
            MenuCategoryId = dto.MenuCategoryId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Calories = dto.Calories,
            WeightGrams = dto.WeightGrams,
            ImageUrl = dto.ImageUrl,
            IsAvailable = dto.IsAvailable,
            TagLinks = dto.TagIds.Select(tid => new MenuItemTagLink
            {
                TagId = tid
            }).ToList()
        };

        await _repo.AddAsync(entity);
    }

    public async Task UpdateAsync(MenuItemDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity is null) return;

        entity.MenuCategoryId = dto.MenuCategoryId;
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.Calories = dto.Calories;
        entity.WeightGrams = dto.WeightGrams;
        entity.ImageUrl = dto.ImageUrl;
        entity.IsAvailable = dto.IsAvailable;

        // оновлення тегів: найпростіше і надійне
        entity.TagLinks.Clear();
        foreach (var tagId in dto.TagIds.Distinct())
            entity.TagLinks.Add(new MenuItemTagLink { TagId = tagId, MenuItemId = entity.Id });

        await _repo.UpdateAsync(entity);
    }

    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);

    private static MenuItemDto MapToDto(MenuItem x) => new()
    {
        Id = x.Id,
        MenuCategoryId = x.MenuCategoryId,
        Name = x.Name,
        Description = x.Description,
        Price = x.Price,
        Calories = x.Calories,
        WeightGrams = x.WeightGrams,
        ImageUrl = x.ImageUrl,
        IsAvailable = x.IsAvailable,
        TagIds = x.TagLinks.Select(t => t.TagId).ToList()
    };
}
