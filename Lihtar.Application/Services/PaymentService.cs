using AutoMapper;
using Lihtar.Application.DTOs;
using Lihtar.Application.Interfaces;
using Lihtar.Domain.Entities;
using Lihtar.Domain.Enums;

namespace Lihtar.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IMapper _mapper;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IOrderRepository orderRepo,
        IMapper mapper)
    {
        _paymentRepo = paymentRepo;
        _orderRepo = orderRepo;
        _mapper = mapper;
    }

    public async Task<List<PaymentDto>> GetByOrderIdAsync(Guid orderId)
    {
        var payments = await _paymentRepo.GetByOrderIdAsync(orderId);
        return _mapper.Map<List<PaymentDto>>(payments);
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id)
    {
        var payment = await _paymentRepo.GetByIdAsync(id);
        return payment == null ? null : _mapper.Map<PaymentDto>(payment);
    }

    public async Task<Guid> CreateFullPaymentAsync(Guid orderId, Guid userId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        var paidQuantities = await GetPaidQuantitiesByOrderAsync(orderId);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            UserId = userId,
            Type = PaymentType.Full,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in order.Items)
        {
            var paidQty = paidQuantities.ContainsKey(item.Id)
                ? paidQuantities[item.Id]
                : 0;

            var remainingQty = item.Quantity - paidQty;

            if (remainingQty <= 0)
                continue;

            payment.Items.Add(new PaymentItem
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                OrderItemId = item.Id,
                Quantity = remainingQty,
                Price = item.Price
            });
        }

        if (payment.Items.Count == 0)
            throw new InvalidOperationException("Nothing left to pay.");

        payment.Amount = payment.Items.Sum(x => x.Quantity * x.Price);

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        return payment.Id;
    }

    public async Task<Guid> CreateSplitPaymentAsync(Guid orderId, Guid userId, Dictionary<Guid, int> selectedItems)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new InvalidOperationException("Order not found.");

        var paidQuantities = await GetPaidQuantitiesByOrderAsync(orderId);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            UserId = userId,
            Type = PaymentType.Split,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var selected in selectedItems)
        {
            var orderItem = order.Items.FirstOrDefault(x => x.Id == selected.Key);

            if (orderItem == null)
                continue;

            if (selected.Value <= 0)
                continue;

            var paidQty = paidQuantities.ContainsKey(orderItem.Id)
                ? paidQuantities[orderItem.Id]
                : 0;

            var remainingQty = orderItem.Quantity - paidQty;

            if (selected.Value > remainingQty)
                throw new InvalidOperationException("Selected quantity is bigger than remaining quantity.");

            payment.Items.Add(new PaymentItem
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                OrderItemId = orderItem.Id,
                Quantity = selected.Value,
                Price = orderItem.Price
            });
        }

        if (payment.Items.Count == 0)
            throw new InvalidOperationException("No items selected for payment.");

        payment.Amount = payment.Items.Sum(x => x.Quantity * x.Price);

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveChangesAsync();

        return payment.Id;
    }

    public async Task ConfirmMockPaymentAsync(Guid paymentId, string cardNumber)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId);

        if (payment == null)
            throw new InvalidOperationException("Payment not found.");

        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.UtcNow;

        var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
        payment.CardMask = digits.Length >= 4
            ? $"**** **** **** {digits[^4..]}"
            : "****";

        await _paymentRepo.SaveChangesAsync();
    }

    public async Task<decimal> GetRemainingAmountByOrderAsync(Guid orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            return 0;

        var paidQuantities = await GetPaidQuantitiesByOrderAsync(orderId);

        decimal remaining = 0;

        foreach (var item in order.Items)
        {
            var paidQty = paidQuantities.ContainsKey(item.Id)
                ? paidQuantities[item.Id]
                : 0;

            var remainingQty = item.Quantity - paidQty;

            if (remainingQty > 0)
            {
                remaining += remainingQty * item.Price;
            }
        }

        return remaining;
    }

    public async Task<Dictionary<Guid, int>> GetPaidQuantitiesByOrderAsync(Guid orderId)
    {
        var payments = await _paymentRepo.GetByOrderIdAsync(orderId);

        return payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .SelectMany(p => p.Items)
            .GroupBy(i => i.OrderItemId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Quantity)
            );
    }
}