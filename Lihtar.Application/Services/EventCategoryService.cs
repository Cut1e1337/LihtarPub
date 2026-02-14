using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Services;

public class EventCategoryService : IEventCategoryService
{
    private readonly IEventCategoryRepository _repo;
    public EventCategoryService(IEventCategoryRepository repo) => _repo = repo;

    public async Task<List<EventCategoryDto>> GetAllAsync()
        => (await _repo.GetAllAsync())
            .Select(x => new EventCategoryDto { Id = x.Id, Name = x.Name, Description = x.Description })
            .ToList();

    public async Task<EventCategoryDto?> GetByIdAsync(Guid id)
    {
        var x = await _repo.GetByIdAsync(id);
        return x is null ? null : new EventCategoryDto { Id = x.Id, Name = x.Name, Description = x.Description };
    }

    public async Task CreateAsync(EventCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required");
        await _repo.AddAsync(new EventCategory { Id = Guid.NewGuid(), Name = dto.Name.Trim(), Description = dto.Description?.Trim() });
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateAsync(EventCategoryDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity is null) throw new InvalidOperationException("EventCategory not found");

        entity.Name = dto.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return;
        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }
}

public class EventService : IEventService
{
    private readonly IEventRepository _repo;
    public EventService(IEventRepository repo) => _repo = repo;

    public async Task<List<EventDto>> GetAllAsync()
        => (await _repo.GetAllAsync())
            .Select(x => new EventDto
            {
                Id = x.Id,
                EventCategoryId = x.EventCategoryId,
                CategoryName = x.EventCategory?.Name ?? "",
                Title = x.Title,
                Description = x.Description,
                EventDate = x.EventDate,
                DurationMinutes = x.DurationMinutes,
                Price = x.Price,
                TotalSeats = x.TotalSeats,
                AvailableSeats = x.AvailableSeats,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive
            }).ToList();

    public async Task<EventDto?> GetByIdAsync(Guid id)
    {
        var x = await _repo.GetByIdAsync(id);
        if (x is null) return null;

        return new EventDto
        {
            Id = x.Id,
            EventCategoryId = x.EventCategoryId,
            CategoryName = x.EventCategory?.Name ?? "",
            Title = x.Title,
            Description = x.Description,
            EventDate = x.EventDate,
            DurationMinutes = x.DurationMinutes,
            Price = x.Price,
            TotalSeats = x.TotalSeats,
            AvailableSeats = x.AvailableSeats,
            ImageUrl = x.ImageUrl,
            IsActive = x.IsActive
        };
    }

    public async Task CreateAsync(EventDto dto)
    {
        if (dto.TotalSeats < 0) throw new ArgumentException("TotalSeats invalid");

        var entity = new Event
        {
            Id = Guid.NewGuid(),
            EventCategoryId = dto.EventCategoryId,
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            EventDate = dto.EventDate,
            DurationMinutes = dto.DurationMinutes,
            Price = dto.Price,
            TotalSeats = dto.TotalSeats,
            AvailableSeats = dto.TotalSeats, // старт
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateAsync(EventDto dto)
    {
        var entity = await _repo.GetByIdAsync(dto.Id);
        if (entity is null) throw new InvalidOperationException("Event not found");

        // якщо зменшив TotalSeats, не дай піти нижче (TotalSeats - sold)
        var sold = entity.TotalSeats - entity.AvailableSeats;
        if (dto.TotalSeats < sold) throw new InvalidOperationException($"Неможливо: вже продано {sold} квитків");

        entity.EventCategoryId = dto.EventCategoryId;
        entity.Title = dto.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.EventDate = dto.EventDate;
        entity.DurationMinutes = dto.DurationMinutes;
        entity.Price = dto.Price;
        entity.TotalSeats = dto.TotalSeats;
        entity.AvailableSeats = dto.TotalSeats - sold; // зберігаємо продажі
        entity.ImageUrl = dto.ImageUrl;
        entity.IsActive = dto.IsActive;

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return;

        // можна заборонити видаляти якщо є квитки
        if (entity.Tickets.Any())
            throw new InvalidOperationException("Неможливо видалити: є куплені квитки");

        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }
}
