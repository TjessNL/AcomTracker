namespace AcomTracker.Application.Services;
 
using AcomTracker.Application.DTOs;
 
public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetByTenantIdAsync(int tenantId);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto);
    Task<bool> UpdateAsync(int id, UpdatePaymentDto dto);
}
 