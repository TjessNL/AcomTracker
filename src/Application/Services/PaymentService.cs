namespace AcomTracker.Application.Services;
 
using AcomTracker.Application.DTOs;
using AcomTracker.Domain.Entities;
using AcomTracker.Domain.Interfaces;
 
public class PaymentService(
    IPaymentRepository payments,
    ITenantRepository tenants) : IPaymentService
{
    private static PaymentDto ToDto(Payment p) => new(
        p.PaymentId,
        p.TenantId,
        p.Date,
        p.Amount,
        p.Method,
        p.Notes);
 
    public async Task<IEnumerable<PaymentDto>> GetByTenantIdAsync(int tenantId)
    {
        var tenant = await tenants.GetByIdAsync(tenantId);
        if (tenant is null)
            throw new KeyNotFoundException($"Tenant {tenantId} not found.");
 
        var all = await payments.GetByTenantIdAsync(tenantId);
        return all.Select(ToDto);
    }
 
    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto)
    {
        var tenant = await tenants.GetByIdAsync(dto.TenantId);
        if (tenant is null)
            throw new KeyNotFoundException($"Tenant {dto.TenantId} not found.");
 
        var payment = new Payment
        {
            TenantId = dto.TenantId,
            Amount   = dto.Amount,
            Date     = dto.Date,
            Method   = dto.Method,
            Notes    = dto.Notes
        };
 
        await payments.AddAsync(payment);
        await payments.SaveChangesAsync();
 
        return ToDto(payment);
    }
 
    public async Task<bool> UpdateAsync(int id, UpdatePaymentDto dto)
    {
        var payment = await payments.GetByIdAsync(id);
        if (payment is null) return false;
 
        payment.Amount = dto.Amount;
        payment.Date   = dto.Date;
        payment.Method = dto.Method;
        payment.Notes  = dto.Notes;
 
        await payments.SaveChangesAsync();
        return true;
    }
}