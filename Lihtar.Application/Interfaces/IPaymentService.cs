using Lihtar.Application.DTOs;

namespace Lihtar.Application.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetByOrderIdAsync(Guid orderId);

    Task<PaymentDto?> GetByIdAsync(Guid id);

    Task<Guid> CreateFullPaymentAsync(Guid orderId, Guid userId);

    Task<Guid> CreateSplitPaymentAsync(Guid orderId, Guid userId, Dictionary<Guid, int> selectedItems);

    Task ConfirmMockPaymentAsync(Guid paymentId, string cardNumber, bool useBonuses);

    Task<Dictionary<Guid, int>> GetPaidQuantitiesByOrderAsync(Guid orderId);

    Task<decimal> GetRemainingAmountByOrderAsync(Guid orderId);
}