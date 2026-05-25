using Lihtar.Application.DTOs;

namespace Lihtar.Application.Interfaces;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync();

    Task<OrderDto?> GetByIdAsync(Guid id);

    Task<OrderDto?> GetActiveByTableIdAsync(Guid tableId);

    Task<Guid> OpenForTableAsync(Guid tableId);

    Task<Guid> OpenFromReservationAsync(Guid reservationId);

    Task AddItemAsync(Guid orderId, Guid menuItemId, int quantity);

    Task UpdateItemQuantityAsync(Guid orderId, Guid orderItemId, int quantity);

    Task RemoveItemAsync(Guid orderId, Guid orderItemId);

    Task CompleteAsync(Guid orderId);

    Task CancelAsync(Guid orderId);
}