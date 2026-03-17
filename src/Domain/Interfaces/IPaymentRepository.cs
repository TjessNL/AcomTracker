namespace AcomTracker.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetByTenantIdAsync(int tenantId);
    Task<Payment?> GetByIdAsync(int id);
    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
}