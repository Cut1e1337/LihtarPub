using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Application.Mappings;
using Lihtar.Application.Services;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lihtar.Tests.Services;

public class ReservationServiceTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ReservationProfile>();
        }, NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_WhenTableDoesNotExist_ShouldThrowException()
    {
        var service = new ReservationService(
            new FakeReservationRepositoryForTests(),
            new FakeTableRepositoryForReservationTests(),
            CreateMapper());

        var dto = new ReservationDto
        {
            UserId = Guid.NewGuid(),
            TableId = Guid.NewGuid(),
            ReservationDate = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(20, 0, 0),
            Comment = "Test"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenTableIsNotActive_ShouldThrowException()
    {
        var tableId = Guid.NewGuid();

        var tableRepo = new FakeTableRepositoryForReservationTests();
        tableRepo.Tables.Add(new Table
        {
            Id = tableId,
            TableNumber = 1,
            Seats = 4,
            IsActive = false
        });

        var service = new ReservationService(
            new FakeReservationRepositoryForTests(),
            tableRepo,
            CreateMapper());

        var dto = new ReservationDto
        {
            UserId = Guid.NewGuid(),
            TableId = tableId,
            ReservationDate = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(20, 0, 0),
            Comment = "Test"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenTimeConflictExists_ShouldThrowException()
    {
        var tableId = Guid.NewGuid();

        var reservationRepo = new FakeReservationRepositoryForTests
        {
            HasConflict = true
        };

        var tableRepo = new FakeTableRepositoryForReservationTests();
        tableRepo.Tables.Add(new Table
        {
            Id = tableId,
            TableNumber = 1,
            Seats = 4,
            IsActive = true
        });

        var service = new ReservationService(
            reservationRepo,
            tableRepo,
            CreateMapper());

        var dto = new ReservationDto
        {
            UserId = Guid.NewGuid(),
            TableId = tableId,
            ReservationDate = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(20, 0, 0),
            Comment = "Test"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsCorrect_ShouldCreateReservation()
    {
        var tableId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reservationRepo = new FakeReservationRepositoryForTests();

        var tableRepo = new FakeTableRepositoryForReservationTests();
        tableRepo.Tables.Add(new Table
        {
            Id = tableId,
            TableNumber = 1,
            Seats = 4,
            IsActive = true
        });

        var service = new ReservationService(
            reservationRepo,
            tableRepo,
            CreateMapper());

        var dto = new ReservationDto
        {
            UserId = userId,
            TableId = tableId,
            ReservationDate = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(20, 0, 0),
            Comment = "Near window"
        };

        await service.CreateAsync(dto);

        Assert.Single(reservationRepo.Reservations);
        Assert.Equal(tableId, reservationRepo.Reservations.First().TableId);
        Assert.Equal(userId, reservationRepo.Reservations.First().UserId);
        Assert.Equal(ReservationStatus.Pending, reservationRepo.Reservations.First().Status);
        Assert.Equal(1, reservationRepo.SaveChangesCount);
    }

    [Fact]
    public async Task CancelAsync_WhenReservationExists_ShouldSetCancelledStatus()
    {
        var reservationId = Guid.NewGuid();

        var reservationRepo = new FakeReservationRepositoryForTests();
        reservationRepo.Reservations.Add(new Reservation
        {
            Id = reservationId,
            Status = ReservationStatus.Pending
        });

        var service = new ReservationService(
            reservationRepo,
            new FakeTableRepositoryForReservationTests(),
            CreateMapper());

        await service.CancelAsync(reservationId);

        Assert.Equal(ReservationStatus.Cancelled, reservationRepo.Reservations.First().Status);
        Assert.Equal(1, reservationRepo.SaveChangesCount);
    }

    [Fact]
    public async Task ConfirmAsync_WhenReservationExists_ShouldSetConfirmedStatus()
    {
        var reservationId = Guid.NewGuid();

        var reservationRepo = new FakeReservationRepositoryForTests();
        reservationRepo.Reservations.Add(new Reservation
        {
            Id = reservationId,
            Status = ReservationStatus.Pending
        });

        var service = new ReservationService(
            reservationRepo,
            new FakeTableRepositoryForReservationTests(),
            CreateMapper());

        await service.ConfirmAsync(reservationId);

        Assert.Equal(ReservationStatus.Confirmed, reservationRepo.Reservations.First().Status);
        Assert.Equal(1, reservationRepo.SaveChangesCount);
    }
}

public class FakeReservationRepositoryForTests : IReservationRepository
{
    public List<Reservation> Reservations { get; } = new();
    public bool HasConflict { get; set; }
    public int SaveChangesCount { get; private set; }

    public Task<List<Reservation>> GetAllAsync()
    {
        return Task.FromResult(Reservations);
    }

    public Task<List<Reservation>> GetByUserIdAsync(Guid userId)
    {
        return Task.FromResult(Reservations.Where(x => x.UserId == userId).ToList());
    }

    public Task<Reservation?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Reservations.FirstOrDefault(x => x.Id == id));
    }

    public Task<bool> HasReservationConflictAsync(
        Guid tableId,
        DateTime reservationDate,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? excludeReservationId = null)
    {
        return Task.FromResult(HasConflict);
    }

    public Task AddAsync(Reservation reservation)
    {
        Reservations.Add(reservation);
        return Task.CompletedTask;
    }

    public void Remove(Reservation reservation)
    {
        Reservations.Remove(reservation);
    }

    public Task SaveChangesAsync()
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}

public class FakeTableRepositoryForReservationTests : ITableRepository
{
    public List<Table> Tables { get; } = new();

    public Task<List<Table>> GetAllAsync()
    {
        return Task.FromResult(Tables);
    }

    public Task<Table?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Tables.FirstOrDefault(x => x.Id == id));
    }

    public Task AddAsync(Table table)
    {
        Tables.Add(table);
        return Task.CompletedTask;
    }

    public void Remove(Table table)
    {
        Tables.Remove(table);
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}