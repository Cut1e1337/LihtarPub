using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repo;
    private readonly IMapper _mapper;

    public MenuItemService(IMenuItemRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<MenuItemDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<MenuItemDto>>(items);
    }

    public async Task<MenuItemDto?> GetByIdAsync(Guid id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item is null ? null : _mapper.Map<MenuItemDto>(item);
    }

    public async Task CreateAsync(MenuItemDto dto)
    {
        var entity = _mapper.Map<MenuItem>(dto);
        entity.Id = Guid.NewGuid();

        // теги: DTO.TagIds -> TagLinks
        entity.TagLinks = dto.TagIds
            .Distinct()
            .Select(tid => new MenuItemTagLink { TagId = tid, MenuItemId = entity.Id })
            .ToList();

        await _repo.AddAsync(entity);
    }

    public async Task UpdateAsync(MenuItemDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity is null) return;

        // мапимо прості поля (без TagLinks)
        _mapper.Map(dto, entity);

        // оновлення тегів як у тебе (надійно)
        var newIds = dto.TagIds.Distinct().ToHashSet();

        // прибираємо зайві
        entity.TagLinks = entity.TagLinks
            .Where(l => newIds.Contains(l.TagId))
            .ToList();

        // додаємо нові
        var existing = entity.TagLinks.Select(l => l.TagId).ToHashSet();
        foreach (var tagId in newIds)
        {
            if (!existing.Contains(tagId))
                entity.TagLinks.Add(new MenuItemTagLink { TagId = tagId, MenuItemId = entity.Id });
        }

        await _repo.UpdateAsync(entity);
    }

    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
}