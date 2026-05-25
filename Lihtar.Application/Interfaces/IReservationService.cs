using Lihtar.Application.DTOs;

namespace Lihtar.Application.Interfaces;

public interface IReservationService
{
    Task<List<ReservationDto>> GetAllAsync();

    Task<List<ReservationDto>> GetByUserIdAsync(Guid userId);

    Task<ReservationDto?> GetByIdAsync(Guid id);

    Task CreateAsync(ReservationDto dto);

    Task ConfirmAsync(Guid id);

    Task CancelAsync(Guid id);

    Task CompleteAsync(Guid id);

    Task DeleteAsync(Guid id);
}