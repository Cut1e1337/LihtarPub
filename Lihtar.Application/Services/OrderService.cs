using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;

namespace Lihtar.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ITableRepository _tableRepo;
    private readonly IReservationRepository _reservationRepo;
    private readonly IMenuItemRepository _menuItemRepo;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepo,
        ITableRepository tableRepo,
        IReservationRepository reservationRepo,
        IMenuItemRepository menuItemRepo,
        IMapper mapper)
    {
        _orderRepo = orderRepo;
        _tableRepo = tableRepo;
        _reservationRepo = reservationRepo;
        _menuItemRepo = menuItemRepo;
        _mapper = mapper;
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepo.GetAllAsync();
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        return order == null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetActiveByTableIdAsync(Guid tableId)
    {
        var order = await _orderRepo.GetActiveByTableIdAsync(tableId);
        return order == null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<Guid> OpenForTableAsync(Guid tableId)
    {
        var table = await _tableRepo.GetByIdAsync(tableId);

        if (table == null)
            throw new InvalidOperationException("Table not found.");

        if (!table.IsActive)
            throw new InvalidOperationException("Table is not active.");

        var activeOrder = await _orderRepo.GetActiveByTableIdAsync(tableId);

        if (activeOrder != null)
            return activeOrder.Id;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            TableId = tableId,
            ReservationId = null,
            UserId = null,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 0
        };

        await _orderRepo.AddAsync(order);
        await _orderRepo.SaveChangesAsync();

        return order.Id;
    }

    public async Task<Guid> OpenFromReservationAsync(Guid reservationId)
    {
        var reservation = await _reservationRepo.GetByIdAsync(reservationId);

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found.");

        var activeOrder = await _orderRepo.GetActiveByTableIdAsync(reservation.TableId);

        if (activeOrder != null)
            return activeOrder.Id;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            TableId = reservation.TableId,
            ReservationId = reservation.Id,
            UserId = reservation.UserId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 0
        };

        reservation.Status = ReservationStatus.Completed;

        await _orderRepo.AddAsync(order);
        await _orderRepo.SaveChangesAsync();

        return order.Id;
    }

    public async Task AddItemAsync(Guid orderId, Guid menuItemId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        var menuItem = await _menuItemRepo.GetByIdAsync(menuItemId);

        if (menuItem == null)
            throw new InvalidOperationException("Menu item not found.");

        var existingItem = order.Items
            .FirstOrDefault(x => x.MenuItemId == menuItemId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var newItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                MenuItemId = menuItem.Id,
                Quantity = quantity,
                Price = menuItem.Price
            };

            await _orderRepo.AddOrderItemAsync(newItem);
        }

        order.TotalPrice = order.Items.Sum(x => x.Quantity * x.Price);

        await _orderRepo.SaveChangesAsync();
    }

    public async Task UpdateItemQuantityAsync(Guid orderId, Guid orderItemId, int quantity)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        var item = order.Items.FirstOrDefault(x => x.Id == orderItemId);

        if (item == null)
            throw new InvalidOperationException("Order item not found.");

        if (quantity <= 0)
        {
            order.Items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        Recalculate(order);

        await _orderRepo.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(Guid orderId, Guid orderItemId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        var item = order.Items.FirstOrDefault(x => x.Id == orderItemId);

        if (item == null)
            return;

        order.Items.Remove(item);

        Recalculate(order);

        await _orderRepo.SaveChangesAsync();
    }

    public async Task CompleteAsync(Guid orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        order.Status = OrderStatus.Completed;
        order.ClosedAt = DateTime.UtcNow;

        Recalculate(order);

        await _orderRepo.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        order.Status = OrderStatus.Cancelled;
        order.ClosedAt = DateTime.UtcNow;

        await _orderRepo.SaveChangesAsync();
    }

    private static void Recalculate(Order order)
    {
        order.TotalPrice = order.Items.Sum(x => x.Quantity * x.Price);
    }
}