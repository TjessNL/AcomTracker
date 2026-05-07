namespace AcomTracker.Domain.Interfaces;

using AcomTracker.Domain.Entities;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetByTenantIdAsync(int tenantId);
    Task<Payment?> GetByIdAsync(int id);
    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
}