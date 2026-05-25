using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class BonusRepository : IBonusRepository
{
    private readonly ArtPubDbContext _db;

    public BonusRepository(ArtPubDbContext db)
    {
        _db = db;
    }

    public async Task<UserProfile?> GetProfileByUserIdAsync(Guid userId)
    {
        return await _db.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddUserProfileAsync(UserProfile profile)
    {
        await _db.UserProfiles.AddAsync(profile);
    }

    public async Task AddBonusTransactionAsync(BonusTransaction transaction)
    {
        await _db.BonusTransactions.AddAsync(transaction);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}