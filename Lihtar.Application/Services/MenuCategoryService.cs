using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Services;

public class MenuCategoryService : IMenuCategoryService
{
    private readonly IMenuCategoryRepository _repo;
    private readonly IMapper _mapper;

    public MenuCategoryService(IMenuCategoryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<MenuCategoryDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<MenuCategoryDto>>(items);
    }

    public async Task<MenuCategoryDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<MenuCategoryDto>(entity);
    }

    public async Task CreateAsync(MenuCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");

        var entity = _mapper.Map<MenuCategory>(dto);

        entity.Id = Guid.NewGuid(); // важливо
        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuCategoryDto dto)
    {
        if (dto.Id == Guid.Empty)
            throw new ArgumentException("Id is required");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");

        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity is null)
            throw new InvalidOperationException("MenuCategory not found");

        // мапимо прості поля (Items не чіпаємо)
        _mapper.Map(dto, entity);

        // нормалізація
        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _repo.GetByIdAsync(id);

        if (category == null)
            return;

        category.IsDeleted = true;

        await _repo.SaveChangesAsync();
    }
}