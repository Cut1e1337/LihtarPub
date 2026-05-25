using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;

namespace Lihtar.Application.Interfaces;

public interface IReservationRepository
{
    Task<List<Reservation>> GetAllAsync();

    Task<List<Reservation>> GetByUserIdAsync(Guid userId);

    Task<Reservation?> GetByIdAsync(Guid id);

    Task<bool> HasReservationConflictAsync(
        Guid tableId,
        DateTime reservationDate,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? ignoreReservationId = null);

    Task AddAsync(Reservation reservation);

    void Remove(Reservation reservation);

    Task SaveChangesAsync();
}