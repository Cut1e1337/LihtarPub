using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Application.Mappings;
using Lihtar.Application.Services;
using Lihtar.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lihtar.Tests.Services;

public class EventCategoryServiceTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventCategoryProfile>();
        }, NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    [Fact]
    public async Task GetAllAsync_WhenCategoriesExist_ShouldReturnAllCategories()
    {
        var repo = new FakeEventCategoryRepository();

        repo.Categories.Add(new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = "Концерт"
        });

        repo.Categories.Add(new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = "Квіз"
        });

        var service = new EventCategoryService(repo, CreateMapper());

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Концерт");
        Assert.Contains(result, x => x.Name == "Квіз");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnCategory()
    {
        var id = Guid.NewGuid();

        var repo = new FakeEventCategoryRepository();

        repo.Categories.Add(new EventCategory
        {
            Id = id,
            Name = "Концерт"
        });

        var service = new EventCategoryService(repo, CreateMapper());

        var result = await service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Концерт", result.Name);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ShouldThrowException()
    {
        var repo = new FakeEventCategoryRepository();
        var service = new EventCategoryService(repo, CreateMapper());

        var dto = new EventCategoryDto
        {
            Name = ""
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsCorrect_ShouldAddCategory()
    {
        var repo = new FakeEventCategoryRepository();
        var service = new EventCategoryService(repo, CreateMapper());

        var dto = new EventCategoryDto
        {
            Name = "  Концерт  ",
            Description = "  Жива музика  "
        };

        await service.CreateAsync(dto);

        Assert.Single(repo.Categories);
        Assert.Equal("Концерт", repo.Categories.First().Name);
        Assert.Equal("Жива музика", repo.Categories.First().Description);
        Assert.Equal(1, repo.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_ShouldThrowException()
    {
        var repo = new FakeEventCategoryRepository();
        var service = new EventCategoryService(repo, CreateMapper());

        var dto = new EventCategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Квіз"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenDataIsCorrect_ShouldUpdateCategory()
    {
        var id = Guid.NewGuid();

        var repo = new FakeEventCategoryRepository();

        repo.Categories.Add(new EventCategory
        {
            Id = id,
            Name = "Old",
            Description = "Old"
        });

        var service = new EventCategoryService(repo, CreateMapper());

        var dto = new EventCategoryDto
        {
            Id = id,
            Name = "  New  ",
            Description = "  New description  "
        };

        await service.UpdateAsync(dto);

        Assert.Equal("New", repo.Categories.First().Name);
        Assert.Equal("New description", repo.Categories.First().Description);
        Assert.Equal(1, repo.SaveChangesCount);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExists_ShouldRemoveCategory()
    {
        var id = Guid.NewGuid();

        var repo = new FakeEventCategoryRepository();

        repo.Categories.Add(new EventCategory
        {
            Id = id,
            Name = "Концерт"
        });

        var service = new EventCategoryService(repo, CreateMapper());

        await service.DeleteAsync(id);

        Assert.Empty(repo.Categories);
        Assert.Equal(1, repo.SaveChangesCount);
    }
}

public class FakeEventCategoryRepository : IEventCategoryRepository
{
    public List<EventCategory> Categories { get; } = new();
    public int SaveChangesCount { get; private set; }

    public Task<List<EventCategory>> GetAllAsync()
    {
        return Task.FromResult(Categories);
    }

    public Task<EventCategory?> GetByIdAsync(Guid id)
    {
        var category = Categories.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(category);
    }

    public Task AddAsync(EventCategory entity)
    {
        Categories.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(EventCategory entity)
    {
        Categories.Remove(entity);
    }

    public Task SaveChangesAsync()
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}