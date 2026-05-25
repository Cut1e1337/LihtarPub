using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ArtPubDbContext _db;

    public PaymentRepository(ArtPubDbContext db)
    {
        _db = db;
    }

    public async Task<List<Payment>> GetByOrderIdAsync(Guid orderId)
    {
        return await _db.Payments
            .Include(x => x.Items)
                .ThenInclude(x => x.OrderItem)
                    .ThenInclude(x => x.MenuItem)
            .Where(x => x.OrderId == orderId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _db.Payments
            .Include(x => x.Items)
                .ThenInclude(x => x.OrderItem)
                    .ThenInclude(x => x.MenuItem)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Payment payment)
    {
        await _db.Payments.AddAsync(payment);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}