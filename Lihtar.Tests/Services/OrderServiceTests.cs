using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Application.Mappings;
using Lihtar.Application.Services;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lihtar.Tests.Services;

public class OrderServiceTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderProfile>();
        }, NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    [Fact]
    public async Task OpenForTableAsync_WhenTableDoesNotExist_ShouldThrowException()
    {
        var orderRepo = new FakeOrderRepository();
        var tableRepo = new FakeOrderTableRepository();
        var reservationRepo = new FakeOrderReservationRepository();
        var menuItemRepo = new FakeOrderMenuItemRepository();

        var service = new OrderService(
            orderRepo,
            tableRepo,
            reservationRepo,
            menuItemRepo,
            CreateMapper());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.OpenForTableAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task OpenForTableAsync_WhenTableIsNotActive_ShouldThrowException()
    {
        var tableId = Guid.NewGuid();

        var tableRepo = new FakeOrderTableRepository();
        tableRepo.Tables.Add(new Table
        {
            Id = tableId,
            TableNumber = 1,
            Seats = 4,
            IsActive = false
        });

        var service = new OrderService(
            new FakeOrderRepository(),
            tableRepo,
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.OpenForTableAsync(tableId));
    }

    [Fact]
    public async Task OpenForTableAsync_WhenActiveOrderExists_ShouldReturnExistingOrderId()
    {
        var tableId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepository();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            TableId = tableId,
            Status = OrderStatus.Draft
        });

        var tableRepo = new FakeOrderTableRepository();
        tableRepo.Tables.Add(new Table
        {
            Id = tableId,
            TableNumber = 1,
            Seats = 4,
            IsActive = true
        });

        var service = new OrderService(
            orderRepo,
            tableRepo,
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        var result = await service.OpenForTableAsync(tableId);

        Assert.Equal(orderId, result);
        Assert.Single(orderRepo.Orders);
    }

    [Fact]
    public async Task OpenForTableAsync_WhenTableIsActive_ShouldCreateNewOrder()
    {
        var tableId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepository();

        var tableRepo = new FakeOrderTableRepository();
        tableRepo.Tables.Add(new Table
        {
            Id = tableId,
            TableNumber = 1,
            Seats = 4,
            IsActive = true
        });

        var service = new OrderService(
            orderRepo,
            tableRepo,
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        var orderId = await service.OpenForTableAsync(tableId);

        Assert.Single(orderRepo.Orders);
        Assert.Equal(orderId, orderRepo.Orders.First().Id);
        Assert.Equal(tableId, orderRepo.Orders.First().TableId);
        Assert.Equal(OrderStatus.Draft, orderRepo.Orders.First().Status);
        Assert.Equal(1, orderRepo.SaveChangesCount);
    }

    [Fact]
    public async Task OpenFromReservationAsync_WhenReservationExists_ShouldCreateOrderAndCompleteReservation()
    {
        var reservationId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reservationRepo = new FakeOrderReservationRepository();
        reservationRepo.Reservations.Add(new Reservation
        {
            Id = reservationId,
            TableId = tableId,
            UserId = userId,
            Status = ReservationStatus.Confirmed
        });

        var orderRepo = new FakeOrderRepository();

        var service = new OrderService(
            orderRepo,
            new FakeOrderTableRepository(),
            reservationRepo,
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        var orderId = await service.OpenFromReservationAsync(reservationId);

        Assert.Single(orderRepo.Orders);
        Assert.Equal(orderId, orderRepo.Orders.First().Id);
        Assert.Equal(tableId, orderRepo.Orders.First().TableId);
        Assert.Equal(reservationId, orderRepo.Orders.First().ReservationId);
        Assert.Equal(userId, orderRepo.Orders.First().UserId);
        Assert.Equal(ReservationStatus.Completed, reservationRepo.Reservations.First().Status);
    }

    [Fact]
    public async Task AddItemAsync_WhenQuantityIsZero_ShouldThrowException()
    {
        var service = new OrderService(
            new FakeOrderRepository(),
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddItemAsync(Guid.NewGuid(), Guid.NewGuid(), 0));
    }

    [Fact]
    public async Task AddItemAsync_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var menuItemRepo = new FakeOrderMenuItemRepository();

        var service = new OrderService(
            new FakeOrderRepository(),
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            menuItemRepo,
            CreateMapper());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddItemAsync(Guid.NewGuid(), Guid.NewGuid(), 1));
    }

    [Fact]
    public async Task AddItemAsync_WhenMenuItemDoesNotExist_ShouldThrowException()
    {
        var orderId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepository();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            Items = new List<OrderItem>()
        });

        var service = new OrderService(
            orderRepo,
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddItemAsync(orderId, Guid.NewGuid(), 1));
    }

    [Fact]
    public async Task AddItemAsync_WhenItemDoesNotExistInOrder_ShouldAddNewItem()
    {
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepository();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            Items = new List<OrderItem>()
        });

        var menuItemRepo = new FakeOrderMenuItemRepository();
        menuItemRepo.MenuItems.Add(new MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Price = 100
        });

        var service = new OrderService(
            orderRepo,
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            menuItemRepo,
            CreateMapper());

        await service.AddItemAsync(orderId, menuItemId, 2);

        Assert.Single(orderRepo.AddedItems);
        Assert.Equal(orderId, orderRepo.AddedItems.First().OrderId);
        Assert.Equal(menuItemId, orderRepo.AddedItems.First().MenuItemId);
        Assert.Equal(2, orderRepo.AddedItems.First().Quantity);
        Assert.Equal(100, orderRepo.AddedItems.First().Price);
        Assert.Equal(1, orderRepo.SaveChangesCount);
    }

    [Fact]
    public async Task AddItemAsync_WhenItemAlreadyExists_ShouldIncreaseQuantity()
    {
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    MenuItemId = menuItemId,
                    Quantity = 1,
                    Price = 100
                }
            }
        };

        var orderRepo = new FakeOrderRepository();
        orderRepo.Orders.Add(order);

        var menuItemRepo = new FakeOrderMenuItemRepository();
        menuItemRepo.MenuItems.Add(new MenuItem
        {
            Id = menuItemId,
            Name = "Burger",
            Price = 100
        });

        var service = new OrderService(
            orderRepo,
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            menuItemRepo,
            CreateMapper());

        await service.AddItemAsync(orderId, menuItemId, 3);

        Assert.Equal(4, order.Items.First().Quantity);
        Assert.Equal(400, order.TotalPrice);
        Assert.Equal(1, orderRepo.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenQuantityIsPositive_ShouldChangeQuantityAndRecalculate()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = orderItemId,
                    OrderId = orderId,
                    Quantity = 2,
                    Price = 50
                }
            }
        };

        var orderRepo = new FakeOrderRepository();
        orderRepo.Orders.Add(order);

        var service = new OrderService(
            orderRepo,
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        await service.UpdateItemQuantityAsync(orderId, orderItemId, 5);

        Assert.Equal(5, order.Items.First().Quantity);
        Assert.Equal(250, order.TotalPrice);
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_WhenQuantityIsZero_ShouldRemoveItem()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = orderItemId,
                    OrderId = orderId,
                    Quantity = 2,
                    Price = 50
                }
            }
        };

        var orderRepo = new FakeOrderRepository();
        orderRepo.Orders.Add(order);

        var service = new OrderService(
            orderRepo,
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        await service.UpdateItemQuantityAsync(orderId, orderItemId, 0);

        Assert.Empty(order.Items);
        Assert.Equal(0, order.TotalPrice);
    }

    [Fact]
    public async Task CompleteAsync_WhenOrderExists_ShouldMarkOrderAsCompleted()
    {
        var orderId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepository();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            Status = OrderStatus.Draft,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Quantity = 2,
                    Price = 100
                }
            }
        });

        var service = new OrderService(
            orderRepo,
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        await service.CompleteAsync(orderId);

        var order = orderRepo.Orders.First();

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.NotNull(order.ClosedAt);
        Assert.Equal(200, order.TotalPrice);
    }

    [Fact]
    public async Task CancelAsync_WhenOrderExists_ShouldMarkOrderAsCancelled()
    {
        var orderId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepository();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            Status = OrderStatus.Draft
        });

        var service = new OrderService(
            orderRepo,
            new FakeOrderTableRepository(),
            new FakeOrderReservationRepository(),
            new FakeOrderMenuItemRepository(),
            CreateMapper());

        await service.CancelAsync(orderId);

        var order = orderRepo.Orders.First();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.ClosedAt);
    }
}

public class FakeOrderRepository : IOrderRepository
{
    public List<Order> Orders { get; } = new();
    public List<OrderItem> AddedItems { get; } = new();
    public int SaveChangesCount { get; private set; }

    public Task<List<Order>> GetAllAsync()
    {
        return Task.FromResult(Orders);
    }

    public Task<Order?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Orders.FirstOrDefault(x => x.Id == id));
    }

    public Task<Order?> GetActiveByTableIdAsync(Guid tableId)
    {
        var order = Orders.FirstOrDefault(x =>
            x.TableId == tableId &&
            x.Status != OrderStatus.Completed &&
            x.Status != OrderStatus.Cancelled);

        return Task.FromResult(order);
    }

    public Task AddAsync(Order order)
    {
        Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    public Task AddOrderItemAsync(OrderItem item)
    {
        AddedItems.Add(item);

        var order = Orders.FirstOrDefault(x => x.Id == item.OrderId);

        if (order is not null)
        {
            order.Items.Add(item);
        }

        return Task.CompletedTask;
    }
}

public class FakeOrderTableRepository : ITableRepository
{
    public List<Table> Tables { get; } = new();

    public Task<List<Table>> GetAllAsync()
    {
        return Task.FromResult(Tables);
    }

    public Task<Table?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Tables.FirstOrDefault(x => x.Id == id));
    }

    public Task AddAsync(Table table)
    {
        Tables.Add(table);
        return Task.CompletedTask;
    }

    public void Remove(Table table)
    {
        Tables.Remove(table);
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}

public class FakeOrderReservationRepository : IReservationRepository
{
    public Task<List<Reservation>> GetByUserIdAsync(Guid userId)
    {
        var result = Reservations
            .Where(x => x.UserId == userId)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<bool> HasReservationConflictAsync(
        Guid tableId,
        DateTime reservationDate,
        TimeSpan startTime,
        TimeSpan endTime,
        Guid? excludeReservationId = null)
    {
        return Task.FromResult(false);
    }
    public List<Reservation> Reservations { get; } = new();

    public Task<List<Reservation>> GetAllAsync()
    {
        return Task.FromResult(Reservations);
    }

    public Task<Reservation?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Reservations.FirstOrDefault(x => x.Id == id));
    }

    public Task AddAsync(Reservation reservation)
    {
        Reservations.Add(reservation);
        return Task.CompletedTask;
    }

    public void Remove(Reservation reservation)
    {
        Reservations.Remove(reservation);
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}

public class FakeOrderMenuItemRepository : IMenuItemRepository
{
    public Task UpdateAsync(MenuItem entity)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var item = MenuItems.FirstOrDefault(x => x.Id == id);

        if (item is not null)
        {
            MenuItems.Remove(item);
        }

        return Task.CompletedTask;
    }
    public List<MenuItem> MenuItems { get; } = new();

    public Task<List<MenuItem>> GetAllAsync()
    {
        return Task.FromResult(MenuItems);
    }

    public Task<MenuItem?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(MenuItems.FirstOrDefault(x => x.Id == id));
    }

    public Task AddAsync(MenuItem entity)
    {
        MenuItems.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(MenuItem entity)
    {
        MenuItems.Remove(entity);
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
}