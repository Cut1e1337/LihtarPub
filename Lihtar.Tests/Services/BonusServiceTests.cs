using Lihtar.Application.Interfaces;
using Lihtar.Application.Services;
using Lihtar.Domain.Entities;

namespace Lihtar.Tests.Services;

public class BonusServiceTests
{
    [Fact]
    public async Task GetUserBonusesAsync_WhenProfileDoesNotExist_ShouldCreateProfileWithZeroBonuses()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeBonusRepository();
        var service = new BonusService(repo);

        var bonuses = await service.GetUserBonusesAsync(userId);

        Assert.Equal(0, bonuses);
        Assert.Single(repo.Profiles);
        Assert.Equal(userId, repo.Profiles.First().UserId);
        Assert.Equal(1, repo.SaveChangesCount);
    }

    [Fact]
    public async Task AddBonusAsync_WhenPointsArePositive_ShouldIncreaseUserBonuses()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeBonusRepository();
        repo.Profiles.Add(new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BonusPoints = 10
        });

        var service = new BonusService(repo);

        await service.AddBonusAsync(userId, 5, "Test bonus");

        var profile = repo.Profiles.First(x => x.UserId == userId);

        Assert.Equal(15, profile.BonusPoints);
        Assert.Single(repo.Transactions);
        Assert.Equal(5, repo.Transactions.First().Points);
        Assert.Equal("Test bonus", repo.Transactions.First().Reason);
    }

    [Fact]
    public async Task AddBonusAsync_WhenPointsAreZero_ShouldDoNothing()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeBonusRepository();
        repo.Profiles.Add(new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BonusPoints = 10
        });

        var service = new BonusService(repo);

        await service.AddBonusAsync(userId, 0, "Zero bonus");

        Assert.Equal(10, repo.Profiles.First().BonusPoints);
        Assert.Empty(repo.Transactions);
    }

    [Fact]
    public async Task UseBonusesAsync_WhenEnoughBonuses_ShouldDecreaseUserBonuses()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeBonusRepository();
        repo.Profiles.Add(new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BonusPoints = 20
        });

        var service = new BonusService(repo);

        await service.UseBonusesAsync(userId, 7, "Use bonus");

        var profile = repo.Profiles.First(x => x.UserId == userId);

        Assert.Equal(13, profile.BonusPoints);
        Assert.Single(repo.Transactions);
        Assert.Equal(-7, repo.Transactions.First().Points);
    }

    [Fact]
    public async Task UseBonusesAsync_WhenNotEnoughBonuses_ShouldThrowException()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeBonusRepository();
        repo.Profiles.Add(new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BonusPoints = 3
        });

        var service = new BonusService(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UseBonusesAsync(userId, 10, "Use too much bonus"));
    }
}

public class FakeBonusRepository : IBonusRepository
{
    public List<UserProfile> Profiles { get; } = new();
    public List<BonusTransaction> Transactions { get; } = new();
    public int SaveChangesCount { get; private set; }

    public Task<UserProfile?> GetProfileByUserIdAsync(Guid userId)
    {
        var profile = Profiles.FirstOrDefault(x => x.UserId == userId);
        return Task.FromResult(profile);
    }

    public Task AddUserProfileAsync(UserProfile profile)
    {
        Profiles.Add(profile);
        return Task.CompletedTask;
    }

    public Task AddBonusTransactionAsync(BonusTransaction transaction)
    {
        Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}