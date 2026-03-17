namespace AcomTracker.Infrastructure.Repositories;

using AcomTracker.Domain.Entities;
using AcomTracker.Domain.Interfaces;
using AcomTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class PaymentRepository(AcomDb db) : IPaymentRepository
{
    public async Task<IEnumerable<Payment>> GetByTenantIdAsync(int tenantId) =>
        await db.Payments
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.Date)
            .ToListAsync();

    public async Task<Payment?> GetByIdAsync(int id) =>
        await db.Payments.FindAsync(id);

    public async Task AddAsync(Payment payment) =>
        await db.Payments.AddAsync(payment);

    public async Task SaveChangesAsync() =>
        await db.SaveChangesAsync();
}