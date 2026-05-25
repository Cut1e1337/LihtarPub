using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ArtPubDbContext _db;

    public ReservationRepository(ArtPubDbContext db)
    {
        _db = db;
    }

    public async Task<List<Reservation>> GetAllAsync()
    {
        return await _db.Reservations
            .Include(x => x.Table)
            .OrderByDescending(x => x.ReservationDate)
            .ThenBy(x => x.StartTime)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetByUserIdAsync(Guid userId)
    {
        return await _db.Reservations
            .Include(x => x.Table)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ReservationDate)
            .ThenBy(x => x.StartTime)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(Guid id)
    {
        return await _db.Reservations
            .Include(x => x.Table)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> HasReservationConflictAsync(
        Guid tableId,
        DateTime reservationDate,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? ignoreReservationId = null)
    {
        return await _db.Reservations
            .AnyAsync(x =>
                x.TableId == tableId &&
                x.ReservationDate.Date == reservationDate.Date &&
                x.Status != ReservationStatus.Cancelled &&
                x.Status != ReservationStatus.Completed &&
                (!ignoreReservationId.HasValue || x.Id != ignoreReservationId.Value) &&
                startTime < x.EndTime &&
                endTime > x.StartTime);
    }

    public async Task AddAsync(Reservation reservation)
    {
        await _db.Reservations.AddAsync(reservation);
    }

    public void Remove(Reservation reservation)
    {
        _db.Reservations.Remove(reservation);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}