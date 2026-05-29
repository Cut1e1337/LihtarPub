using Lihtar.Application.Interfaces;
using Lihtar.Application.Services;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;

namespace Lihtar.Tests.Services;

public class PaymentServiceTests
{
    [Fact]
    public async Task CreateFullPaymentAsync_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var service = new PaymentService(
            new FakePaymentRepositoryForPaymentTests(),
            new FakeOrderRepositoryForPaymentTests(),
            null!,
            new FakeBonusServiceForPaymentTests());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateFullPaymentAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateFullPaymentAsync_WhenOrderHasItems_ShouldCreatePendingFullPayment()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepositoryForPaymentTests();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            UserId = userId,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = orderItemId,
                    OrderId = orderId,
                    Quantity = 2,
                    Price = 100
                }
            }
        });

        var paymentRepo = new FakePaymentRepositoryForPaymentTests();

        var service = new PaymentService(
            paymentRepo,
            orderRepo,
            null!,
            new FakeBonusServiceForPaymentTests());

        var paymentId = await service.CreateFullPaymentAsync(orderId, userId);

        var payment = paymentRepo.Payments.Single();

        Assert.Equal(paymentId, payment.Id);
        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(userId, payment.UserId);
        Assert.Equal(PaymentType.Full, payment.Type);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(200, payment.Amount);
        Assert.Single(payment.Items);
        Assert.Equal(orderItemId, payment.Items.First().OrderItemId);
        Assert.Equal(2, payment.Items.First().Quantity);
    }

    [Fact]
    public async Task CreateFullPaymentAsync_WhenEverythingAlreadyPaid_ShouldThrowException()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepositoryForPaymentTests();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            UserId = userId,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = orderItemId,
                    OrderId = orderId,
                    Quantity = 1,
                    Price = 100
                }
            }
        });

        var paymentRepo = new FakePaymentRepositoryForPaymentTests();
        paymentRepo.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            Status = PaymentStatus.Paid,
            Amount = 100,
            Items = new List<PaymentItem>
            {
                new PaymentItem
                {
                    Id = Guid.NewGuid(),
                    OrderItemId = orderItemId,
                    Quantity = 1,
                    Price = 100
                }
            }
        });

        var service = new PaymentService(
            paymentRepo,
            orderRepo,
            null!,
            new FakeBonusServiceForPaymentTests());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateFullPaymentAsync(orderId, userId));
    }

    [Fact]
    public async Task CreateSplitPaymentAsync_WhenSelectedQuantityBiggerThanRemaining_ShouldThrowException()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepositoryForPaymentTests();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = orderItemId,
                    OrderId = orderId,
                    Quantity = 1,
                    Price = 150
                }
            }
        });

        var service = new PaymentService(
            new FakePaymentRepositoryForPaymentTests(),
            orderRepo,
            null!,
            new FakeBonusServiceForPaymentTests());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSplitPaymentAsync(
                orderId,
                userId,
                new Dictionary<Guid, int>
                {
                    [orderItemId] = 2
                }));
    }

    [Fact]
    public async Task CreateSplitPaymentAsync_WhenDataIsCorrect_ShouldCreateSplitPayment()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var firstItemId = Guid.NewGuid();
        var secondItemId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepositoryForPaymentTests();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = firstItemId,
                    OrderId = orderId,
                    Quantity = 3,
                    Price = 100
                },
                new OrderItem
                {
                    Id = secondItemId,
                    OrderId = orderId,
                    Quantity = 1,
                    Price = 50
                }
            }
        });

        var paymentRepo = new FakePaymentRepositoryForPaymentTests();

        var service = new PaymentService(
            paymentRepo,
            orderRepo,
            null!,
            new FakeBonusServiceForPaymentTests());

        var paymentId = await service.CreateSplitPaymentAsync(
            orderId,
            userId,
            new Dictionary<Guid, int>
            {
                [firstItemId] = 2
            });

        var payment = paymentRepo.Payments.Single();

        Assert.Equal(paymentId, payment.Id);
        Assert.Equal(PaymentType.Split, payment.Type);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(200, payment.Amount);
        Assert.Single(payment.Items);
        Assert.Equal(firstItemId, payment.Items.First().OrderItemId);
        Assert.Equal(2, payment.Items.First().Quantity);
    }

    [Fact]
    public async Task ConfirmMockPaymentAsync_WhenPaymentDoesNotExist_ShouldThrowException()
    {
        var service = new PaymentService(
            new FakePaymentRepositoryForPaymentTests(),
            new FakeOrderRepositoryForPaymentTests(),
            null!,
            new FakeBonusServiceForPaymentTests());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConfirmMockPaymentAsync(Guid.NewGuid(), "1111222233334444", false));
    }

    [Fact]
    public async Task ConfirmMockPaymentAsync_WhenPaymentIsPending_ShouldMarkAsPaidAndAddBonus()
    {
        var userId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var paymentRepo = new FakePaymentRepositoryForPaymentTests();
        paymentRepo.Payments.Add(new Payment
        {
            Id = paymentId,
            UserId = userId,
            Amount = 250,
            Status = PaymentStatus.Pending
        });

        var bonusService = new FakeBonusServiceForPaymentTests();

        var service = new PaymentService(
            paymentRepo,
            new FakeOrderRepositoryForPaymentTests(),
            null!,
            bonusService);

        await service.ConfirmMockPaymentAsync(paymentId, "1111 2222 3333 4444", false);

        var payment = paymentRepo.Payments.Single();

        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.NotNull(payment.PaidAt);
        Assert.Equal("**** **** **** 4444", payment.CardMask);
        Assert.Equal(2, bonusService.AddedBonuses);
        Assert.Equal(1, paymentRepo.SaveChangesCount);
    }

    [Fact]
    public async Task ConfirmMockPaymentAsync_WhenUseBonusesIsTrue_ShouldUseBonusesAndAddBonusFromRemainingAmount()
    {
        var userId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var paymentRepo = new FakePaymentRepositoryForPaymentTests();
        paymentRepo.Payments.Add(new Payment
        {
            Id = paymentId,
            UserId = userId,
            Amount = 500,
            Status = PaymentStatus.Pending
        });

        var bonusService = new FakeBonusServiceForPaymentTests
        {
            CurrentBonuses = 100
        };

        var service = new PaymentService(
            paymentRepo,
            new FakeOrderRepositoryForPaymentTests(),
            null!,
            bonusService);

        await service.ConfirmMockPaymentAsync(paymentId, "123456789999", true);

        var payment = paymentRepo.Payments.Single();

        Assert.Equal(PaymentStatus.Paid, payment.Status);
        Assert.Equal(400, payment.Amount);
        Assert.Equal(100, bonusService.UsedBonuses);
        Assert.Equal(4, bonusService.AddedBonuses);
    }

    [Fact]
    public async Task ConfirmMockPaymentAsync_WhenPaymentAlreadyPaid_ShouldDoNothing()
    {
        var userId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var paymentRepo = new FakePaymentRepositoryForPaymentTests();
        paymentRepo.Payments.Add(new Payment
        {
            Id = paymentId,
            UserId = userId,
            Amount = 300,
            Status = PaymentStatus.Paid
        });

        var bonusService = new FakeBonusServiceForPaymentTests();

        var service = new PaymentService(
            paymentRepo,
            new FakeOrderRepositoryForPaymentTests(),
            null!,
            bonusService);

        await service.ConfirmMockPaymentAsync(paymentId, "4444", false);

        Assert.Equal(0, bonusService.AddedBonuses);
        Assert.Equal(0, paymentRepo.SaveChangesCount);
    }

    [Fact]
    public async Task GetRemainingAmountByOrderAsync_WhenPartiallyPaid_ShouldReturnOnlyUnpaidAmount()
    {
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var orderRepo = new FakeOrderRepositoryForPaymentTests();
        orderRepo.Orders.Add(new Order
        {
            Id = orderId,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = orderItemId,
                    OrderId = orderId,
                    Quantity = 3,
                    Price = 100
                }
            }
        });

        var paymentRepo = new FakePaymentRepositoryForPaymentTests();
        paymentRepo.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = PaymentStatus.Paid,
            Items = new List<PaymentItem>
            {
                new PaymentItem
                {
                    Id = Guid.NewGuid(),
                    OrderItemId = orderItemId,
                    Quantity = 1,
                    Price = 100
                }
            }
        });

        var service = new PaymentService(
            paymentRepo,
            orderRepo,
            null!,
            new FakeBonusServiceForPaymentTests());

        var remaining = await service.GetRemainingAmountByOrderAsync(orderId);

        Assert.Equal(200, remaining);
    }
}

public class FakePaymentRepositoryForPaymentTests : IPaymentRepository
{
    public List<Payment> Payments { get; } = new();
    public int SaveChangesCount { get; private set; }

    public Task<List<Payment>> GetByOrderIdAsync(Guid orderId)
    {
        var result = Payments
            .Where(x => x.OrderId == orderId)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Payment?> GetByIdAsync(Guid id)
    {
        var payment = Payments.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(payment);
    }

    public Task AddAsync(Payment payment)
    {
        Payments.Add(payment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}

public class FakeOrderRepositoryForPaymentTests : IOrderRepository
{
    public List<Order> Orders { get; } = new();

    public Task<List<Order>> GetAllAsync()
    {
        return Task.FromResult(Orders);
    }

    public Task<Order?> GetByIdAsync(Guid id)
    {
        var order = Orders.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(order);
    }

    public Task<Order?> GetActiveByTableIdAsync(Guid tableId)
    {
        var order = Orders.FirstOrDefault(x => x.TableId == tableId);
        return Task.FromResult(order);
    }

    public Task AddAsync(Order order)
    {
        Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }

    public Task AddOrderItemAsync(OrderItem item)
    {
        var order = Orders.FirstOrDefault(x => x.Id == item.OrderId);

        if (order is not null)
        {
            order.Items.Add(item);
        }

        return Task.CompletedTask;
    }
}

public class FakeBonusServiceForPaymentTests : IBonusService
{
    public int CurrentBonuses { get; set; }
    public int AddedBonuses { get; private set; }
    public int UsedBonuses { get; private set; }

    public Task<int> GetUserBonusesAsync(Guid userId)
    {
        return Task.FromResult(CurrentBonuses);
    }

    public Task AddBonusAsync(Guid userId, int points, string reason)
    {
        AddedBonuses += points;
        return Task.CompletedTask;
    }

    public Task UseBonusesAsync(Guid userId, int points, string reason)
    {
        UsedBonuses += points;
        CurrentBonuses -= points;
        return Task.CompletedTask;
    }
}