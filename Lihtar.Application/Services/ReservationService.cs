using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;

namespace Lihtar.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _repo;
    private readonly ITableRepository _tableRepo;
    private readonly IMapper _mapper;

    public ReservationService(
        IReservationRepository repo,
        ITableRepository tableRepo,
        IMapper mapper)
    {
        _repo = repo;
        _tableRepo = tableRepo;
        _mapper = mapper;
    }

    public async Task<List<ReservationDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<ReservationDto>>(items);
    }

    public async Task<List<ReservationDto>> GetByUserIdAsync(Guid userId)
    {
        var items = await _repo.GetByUserIdAsync(userId);
        return _mapper.Map<List<ReservationDto>>(items);
    }

    public async Task<ReservationDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<ReservationDto>(entity);
    }

    public async Task CreateAsync(ReservationDto dto)
    {
        if (dto.UserId == Guid.Empty)
            throw new ArgumentException("User is required");

        if (dto.TableId == Guid.Empty)
            throw new ArgumentException("Table is required");

        if (dto.ReservationDate.Date < DateTime.Today)
            throw new ArgumentException("Reservation date cannot be in the past");

        if (dto.StartTime >= dto.EndTime)
            throw new ArgumentException("Start time must be earlier than end time");

        var table = await _tableRepo.GetByIdAsync(dto.TableId);

        if (table == null)
            throw new InvalidOperationException("Table not found");

        if (!table.IsActive)
            throw new InvalidOperationException("This table is not active");

        var hasConflict = await _repo.HasReservationConflictAsync(
            dto.TableId,
            dto.ReservationDate,
            dto.StartTime,
            dto.EndTime);

        if (hasConflict)
            throw new InvalidOperationException("This table is already reserved for the selected time");

        var entity = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            TableId = dto.TableId,
            ReservationDate = dto.ReservationDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
    }

    public async Task ConfirmAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity == null)
            throw new InvalidOperationException("Reservation not found");

        entity.Status = ReservationStatus.Confirmed;

        await _repo.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity == null)
            throw new InvalidOperationException("Reservation not found");

        entity.Status = ReservationStatus.Cancelled;

        await _repo.SaveChangesAsync();
    }

    public async Task CompleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity == null)
            throw new InvalidOperationException("Reservation not found");

        entity.Status = ReservationStatus.Completed;

        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);

        if (entity == null)
            return;

        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
    }
}