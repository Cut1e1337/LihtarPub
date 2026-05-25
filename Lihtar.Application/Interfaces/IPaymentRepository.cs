using Lihtar.Domain.Entities;

namespace Lihtar.Application.Interfaces;

public interface IPaymentRepository
{
    Task<List<Payment>> GetByOrderIdAsync(Guid orderId);

    Task<Payment?> GetByIdAsync(Guid id);

    Task AddAsync(Payment payment);

    Task SaveChangesAsync();
}