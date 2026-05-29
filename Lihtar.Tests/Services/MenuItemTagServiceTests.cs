using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Application.Services;
using Lihtar.Domain.Entities;

namespace Lihtar.Tests.Services;

public class MenuItemTagServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenDataIsCorrect_ShouldAddTag()
    {
        var repo = new FakeMenuItemTagRepository();
        var service = new MenuItemTagService(repo);

        var dto = new MenuItemTagDto
        {
            Name = "Гостре"
        };

        await service.CreateAsync(dto);

        Assert.Single(repo.Tags);
        Assert.Equal("Гостре", repo.Tags.First().Name);
        Assert.NotEqual(Guid.Empty, repo.Tags.First().Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenTagsExist_ShouldReturnAllTags()
    {
        var repo = new FakeMenuItemTagRepository();

        repo.Tags.Add(new MenuItemTag
        {
            Id = Guid.NewGuid(),
            Name = "Гостре"
        });

        repo.Tags.Add(new MenuItemTag
        {
            Id = Guid.NewGuid(),
            Name = "Веганське"
        });

        var service = new MenuItemTagService(repo);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Гостре");
        Assert.Contains(result, x => x.Name == "Веганське");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTagExists_ShouldReturnTag()
    {
        var id = Guid.NewGuid();

        var repo = new FakeMenuItemTagRepository();

        repo.Tags.Add(new MenuItemTag
        {
            Id = id,
            Name = "Новинка"
        });

        var service = new MenuItemTagService(repo);

        var result = await service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Новинка", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTagDoesNotExist_ShouldReturnNull()
    {
        var repo = new FakeMenuItemTagRepository();
        var service = new MenuItemTagService(repo);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenTagExists_ShouldUpdateTag()
    {
        var id = Guid.NewGuid();

        var repo = new FakeMenuItemTagRepository();

        repo.Tags.Add(new MenuItemTag
        {
            Id = id,
            Name = "Old"
        });

        var service = new MenuItemTagService(repo);

        var dto = new MenuItemTagDto
        {
            Id = id,
            Name = "New"
        };

        await service.UpdateAsync(dto);

        Assert.Equal("New", repo.Tags.First().Name);
        Assert.Equal(1, repo.UpdateCount);
    }

    [Fact]
    public async Task UpdateAsync_WhenTagDoesNotExist_ShouldDoNothing()
    {
        var repo = new FakeMenuItemTagRepository();
        var service = new MenuItemTagService(repo);

        var dto = new MenuItemTagDto
        {
            Id = Guid.NewGuid(),
            Name = "New"
        };

        await service.UpdateAsync(dto);

        Assert.Empty(repo.Tags);
        Assert.Equal(0, repo.UpdateCount);
    }

    [Fact]
    public async Task DeleteAsync_WhenTagExists_ShouldRemoveTag()
    {
        var id = Guid.NewGuid();

        var repo = new FakeMenuItemTagRepository();

        repo.Tags.Add(new MenuItemTag
        {
            Id = id,
            Name = "Гостре"
        });

        var service = new MenuItemTagService(repo);

        await service.DeleteAsync(id);

        Assert.Empty(repo.Tags);
        Assert.Equal(1, repo.DeleteCount);
    }
}

public class FakeMenuItemTagRepository : IMenuItemTagRepository
{
    public List<MenuItemTag> Tags { get; } = new();

    public int UpdateCount { get; private set; }
    public int DeleteCount { get; private set; }

    public Task<List<MenuItemTag>> GetAllAsync()
    {
        return Task.FromResult(Tags);
    }

    public Task<MenuItemTag?> GetByIdAsync(Guid id)
    {
        var tag = Tags.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(tag);
    }

    public Task AddAsync(MenuItemTag tag)
    {
        Tags.Add(tag);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MenuItemTag tag)
    {
        UpdateCount++;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var tag = Tags.FirstOrDefault(x => x.Id == id);

        if (tag is not null)
        {
            Tags.Remove(tag);
        }

        DeleteCount++;
        return Task.CompletedTask;
    }
}