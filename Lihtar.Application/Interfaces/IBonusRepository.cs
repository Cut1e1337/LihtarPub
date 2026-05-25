using Lihtar.Domain.Entities;

namespace Lihtar.Application.Interfaces;

public interface IBonusRepository
{
    Task<UserProfile?> GetProfileByUserIdAsync(Guid userId);

    Task AddUserProfileAsync(UserProfile profile);

    Task AddBonusTransactionAsync(BonusTransaction transaction);

    Task SaveChangesAsync();
}