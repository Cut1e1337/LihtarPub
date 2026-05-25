using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _repo;
    private readonly IMapper _mapper;

    public TableService(ITableRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<TableDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<TableDto>>(items);
    }

    public async Task<TableDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TableDto>(entity);
    }

    public async Task CreateAsync(TableDto dto)
    {
        if (dto.TableNumber <= 0)
            throw new ArgumentException("Table number must be greater than zero");

        if (dto.Seats <= 0)
            throw new ArgumentException("Seats must be greater than zero");

        var entity = _mapper.Map<Table>(dto);

        entity.Id = Guid.NewGuid();
        entity.IsActive = dto.IsActive;

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateAsync(TableDto dto)
    {
        if (dto.Id == Guid.Empty)
            throw new ArgumentException("Id is required");

        if (dto.TableNumber <= 0)
            throw new ArgumentException("Table number must be greater than zero");

        if (dto.Seats <= 0)
            throw new ArgumentException("Seats must be greater than zero");

        var entity = await _repo.GetByIdAsync(dto.Id);

        if (entity is null)
            throw new InvalidOperationException("Table not found");

        _mapper.Map(dto, entity);

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity is null)
            return;

        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }
}