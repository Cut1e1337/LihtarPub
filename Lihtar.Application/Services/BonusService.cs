using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;

namespace Lihtar.Application.Services;

public class BonusService : IBonusService
{
    private readonly IBonusRepository _repo;

    public BonusService(IBonusRepository repo)
    {
        _repo = repo;
    }

    public async Task AddBonusAsync(Guid userId, int points, string reason)
    {
        if (points <= 0)
            return;

        var profile = await _repo.GetProfileByUserIdAsync(userId);

        if (profile == null)
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BonusPoints = 0
            };

            await _repo.AddUserProfileAsync(profile);
        }

        profile.BonusPoints += points;

        await _repo.AddBonusTransactionAsync(new BonusTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Points = points,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        });

        await _repo.SaveChangesAsync();
    }
}