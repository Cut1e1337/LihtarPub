using Lihtar.Domain.Entities;

namespace Lihtar.Application.Interfaces;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();

    Task<Order?> GetByIdAsync(Guid id);

    Task<Order?> GetActiveByTableIdAsync(Guid tableId);

    Task AddAsync(Order order);

    Task SaveChangesAsync();

    Task AddOrderItemAsync(OrderItem item);
}