namespace Lihtar.Application.Interfaces;

public interface IBonusService
{
    Task AddBonusAsync(Guid userId, int points, string reason);

    Task<int> GetUserBonusesAsync(Guid userId);

    Task UseBonusesAsync(Guid userId, int points, string reason);
}