using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Application.Mappings;
using Lihtar.Application.Services;
using Lihtar.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lihtar.Tests.Services;

public class TableServiceTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TableProfile>();
        }, NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_WhenTableNumberIsZero_ShouldThrowException()
    {
        var repo = new FakeTableRepository();
        var service = new TableService(repo, CreateMapper());

        var dto = new TableDto
        {
            TableNumber = 0,
            Seats = 2,
            IsActive = true
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenSeatsIsZero_ShouldThrowException()
    {
        var repo = new FakeTableRepository();
        var service = new TableService(repo, CreateMapper());

        var dto = new TableDto
        {
            TableNumber = 1,
            Seats = 0,
            IsActive = true
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsCorrect_ShouldAddTable()
    {
        var repo = new FakeTableRepository();
        var service = new TableService(repo, CreateMapper());

        var dto = new TableDto
        {
            TableNumber = 1,
            Seats = 4,
            IsVip = true,
            IsActive = true
        };

        await service.CreateAsync(dto);

        Assert.Single(repo.Tables);
        Assert.Equal(1, repo.Tables.First().TableNumber);
        Assert.Equal(4, repo.Tables.First().Seats);
        Assert.True(repo.Tables.First().IsVip);
        Assert.True(repo.Tables.First().IsActive);
        Assert.NotEqual(Guid.Empty, repo.Tables.First().Id);
        Assert.Equal(1, repo.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateAsync_WhenTableDoesNotExist_ShouldThrowException()
    {
        var repo = new FakeTableRepository();
        var service = new TableService(repo, CreateMapper());

        var dto = new TableDto
        {
            Id = Guid.NewGuid(),
            TableNumber = 1,
            Seats = 4,
            IsActive = true
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenDataIsCorrect_ShouldUpdateTable()
    {
        var id = Guid.NewGuid();

        var repo = new FakeTableRepository();
        repo.Tables.Add(new Table
        {
            Id = id,
            TableNumber = 1,
            Seats = 2,
            IsVip = false,
            IsActive = true
        });

        var service = new TableService(repo, CreateMapper());

        var dto = new TableDto
        {
            Id = id,
            TableNumber = 5,
            Seats = 6,
            IsVip = true,
            IsActive = false
        };

        await service.UpdateAsync(dto);

        var table = repo.Tables.First();

        Assert.Equal(5, table.TableNumber);
        Assert.Equal(6, table.Seats);
        Assert.True(table.IsVip);
        Assert.False(table.IsActive);
        Assert.Equal(1, repo.SaveChangesCount);
    }

    [Fact]
    public async Task DeleteAsync_WhenTableExists_ShouldRemoveTable()
    {
        var id = Guid.NewGuid();

        var repo = new FakeTableRepository();
        repo.Tables.Add(new Table
        {
            Id = id,
            TableNumber = 1,
            Seats = 4,
            IsActive = true
        });

        var service = new TableService(repo, CreateMapper());

        await service.DeleteAsync(id);

        Assert.Empty(repo.Tables);
        Assert.Equal(1, repo.SaveChangesCount);
    }
}

public class FakeTableRepository : ITableRepository
{
    public List<Table> Tables { get; } = new();
    public int SaveChangesCount { get; private set; }

    public Task<List<Table>> GetAllAsync()
    {
        return Task.FromResult(Tables);
    }

    public Task<Table?> GetByIdAsync(Guid id)
    {
        var table = Tables.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(table);
    }

    public Task AddAsync(Table entity)
    {
        Tables.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(Table entity)
    {
        Tables.Remove(entity);
    }

    public Task SaveChangesAsync()
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}