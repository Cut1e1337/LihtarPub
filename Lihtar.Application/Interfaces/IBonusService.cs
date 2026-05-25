namespace Lihtar.Application.Interfaces;

public interface IBonusService
{
    Task AddBonusAsync(Guid userId, int points, string reason);
}