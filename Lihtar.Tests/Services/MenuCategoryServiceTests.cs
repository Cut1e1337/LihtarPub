
using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Application.Mappings;
using Lihtar.Application.Services;
using Lihtar.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lihtar.Tests.Services;

public class MenuCategoryServiceTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MenuCategoryProfile>();
        }, NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ShouldThrowException()
    {
        var repo = new FakeMenuCategoryRepository();
        var service = new MenuCategoryService(repo, CreateMapper());

        var dto = new MenuCategoryDto
        {
            Name = "",
            SortOrder = 1,
            IsActive = true
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsCorrect_ShouldAddCategory()
    {
        var repo = new FakeMenuCategoryRepository();
        var service = new MenuCategoryService(repo, CreateMapper());

        var dto = new MenuCategoryDto
        {
            Name = "  Напої  ",
            Description = "  Гарячі та холодні напої  ",
            SortOrder = 1,
            IsActive = true
        };

        await service.CreateAsync(dto);

        Assert.Single(repo.Categories);
        Assert.Equal("Напої", repo.Categories.First().Name);
        Assert.Equal("Гарячі та холодні напої", repo.Categories.First().Description);
        Assert.NotEqual(Guid.Empty, repo.Categories.First().Id);
        Assert.Equal(1, repo.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowException()
    {
        var repo = new FakeMenuCategoryRepository();
        var service = new MenuCategoryService(repo, CreateMapper());

        var dto = new MenuCategoryDto
        {
            Id = Guid.Empty,
            Name = "Напої"
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryDoesNotExist_ShouldThrowException()
    {
        var repo = new FakeMenuCategoryRepository();
        var service = new MenuCategoryService(repo, CreateMapper());

        var dto = new MenuCategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Напої"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenDataIsCorrect_ShouldUpdateCategory()
    {
        var id = Guid.NewGuid();

        var repo = new FakeMenuCategoryRepository();
        repo.Categories.Add(new MenuCategory
        {
            Id = id,
            Name = "Old name",
            Description = "Old description",
            SortOrder = 1,
            IsActive = true
        });

        var service = new MenuCategoryService(repo, CreateMapper());

        var dto = new MenuCategoryDto
        {
            Id = id,
            Name = "  New name  ",
            Description = "  New description  ",
            SortOrder = 2,
            IsActive = false
        };

        await service.UpdateAsync(dto);

        var category = repo.Categories.First();

        Assert.Equal("New name", category.Name);
        Assert.Equal("New description", category.Description);
        Assert.Equal(2, category.SortOrder);
        Assert.False(category.IsActive);
        Assert.Equal(1, repo.SaveChangesCount);
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExists_ShouldRemoveCategory()
    {
        var id = Guid.NewGuid();

        var repo = new FakeMenuCategoryRepository();
        repo.Categories.Add(new MenuCategory
        {
            Id = id,
            Name = "Напої"
        });

        var service = new MenuCategoryService(repo, CreateMapper());

        await service.DeleteAsync(id);

        Assert.Empty(repo.Categories);
        Assert.Equal(1, repo.SaveChangesCount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnDto()
    {
        var id = Guid.NewGuid();

        var repo = new FakeMenuCategoryRepository();
        repo.Categories.Add(new MenuCategory
        {
            Id = id,
            Name = "Напої",
            SortOrder = 1,
            IsActive = true
        });

        var service = new MenuCategoryService(repo, CreateMapper());

        var result = await service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Напої", result.Name);
    }
}

public class FakeMenuCategoryRepository : IMenuCategoryRepository
{
    public List<MenuCategory> Categories { get; } = new();
    public int SaveChangesCount { get; private set; }

    public Task<List<MenuCategory>> GetAllAsync()
    {
        return Task.FromResult(Categories);
    }

    public Task<MenuCategory?> GetByIdAsync(Guid id)
    {
        var category = Categories.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(category);
    }

    public Task AddAsync(MenuCategory entity)
    {
        Categories.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(MenuCategory entity)
    {
        Categories.Remove(entity);
    }

    public Task SaveChangesAsync()
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}

