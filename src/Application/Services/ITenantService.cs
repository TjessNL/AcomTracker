namespace AcomTracker.Application.Services;

using AcomTracker.Application.DTOs;

public interface ITenantService
{
    Task<IEnumerable<TenantSummaryDto>> GetDashboardAsync(string? search);
    Task<IEnumerable<TenantListDto>> GetAllAsync(string? search);
    Task<TenantDetailDto?> GetByIdAsync(int id);
    Task<TenantDetailDto> CreateAsync(CreateTenantDto dto);
    Task<bool> DeactivateAsync(int id);
    Task<bool> ReactivateAsync(int id);
}
