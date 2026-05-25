using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;
using Lihtar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lihtar.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ArtPubDbContext _db;

    public OrderRepository(ArtPubDbContext db)
    {
        _db = db;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _db.Orders
            .AsTracking()
            .Include(x => x.Table)
            .Include(x => x.Reservation)
            .Include(x => x.Items)
                .ThenInclude(i => i.MenuItem)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _db.Orders
            .AsTracking()
            .Include(x => x.Table)
            .Include(x => x.Reservation)
            .Include(x => x.Items)
                .ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Order?> GetActiveByTableIdAsync(Guid tableId)
    {
        return await _db.Orders
            .AsTracking()
            .Include(x => x.Table)
            .Include(x => x.Reservation)
            .Include(x => x.Items)
                .ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(x =>
                x.TableId == tableId &&
                (x.Status == OrderStatus.Draft ||
                 x.Status == OrderStatus.Created));
    }

    public async Task AddAsync(Order order)
    {
        await _db.Orders.AddAsync(order);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    public async Task AddOrderItemAsync(OrderItem item)
    {
        await _db.OrderItems.AddAsync(item);
    }
}