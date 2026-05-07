namespace AcomTracker.Application.Services;

using AcomTracker.Application.DTOs;
using AcomTracker.Domain.Entities;
using AcomTracker.Domain.Interfaces;

public class TenantService(ITenantRepository repo) : ITenantService
{
    private static int MonthsElapsed(DateOnly start)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return (today.Year - start.Year) * 12
             + (today.Month - start.Month) + 1;
    }

    private static TenantDetailDto ToDetail(Tenant t)
    {
        var months  = MonthsElapsed(t.LeaseStartDate);
        var charged = t.MonthlyRent * months;
        var paid    = t.Payments.Sum(p => p.Amount);
        return new TenantDetailDto(
            t.TenantId, t.Name, t.MonthlyRent, t.LeaseStartDate,
            paid, charged, charged - paid,
            t.Payments.OrderByDescending(p => p.Date)
                      .Select(p => (DateOnly?)p.Date)
                      .FirstOrDefault());
    }

    public async Task<IEnumerable<TenantSummaryDto>> GetDashboardAsync(string? search)
    {
        var tenants = await repo.GetAllAsync(search);
        return tenants.Select(t =>
        {
            var months  = MonthsElapsed(t.LeaseStartDate);
            var charged = t.MonthlyRent * months;
            var paid    = t.Payments.Sum(p => p.Amount);
            return new TenantSummaryDto(
                t.TenantId, t.Name, t.MonthlyRent, paid, charged, charged - paid,
                t.Payments.OrderByDescending(p => p.Date)
                          .Select(p => (DateOnly?)p.Date)
                          .FirstOrDefault());
        });
    }

    public async Task<IEnumerable<TenantListDto>> GetAllAsync(string? search)
    {
        var tenants = await repo.GetAllAsync(search);
        return tenants.Select(t =>
            new TenantListDto(t.TenantId, t.Name, t.MonthlyRent, t.LeaseStartDate, t.IsActive));
    }

    public async Task<TenantDetailDto?> GetByIdAsync(int id)
    {
        var tenant = await repo.GetByIdAsync(id);
        return tenant is null ? null : ToDetail(tenant);
    }

    public async Task<TenantDetailDto> CreateAsync(CreateTenantDto dto)
    {
        if (await repo.ExistsByNameAsync(dto.Name))
            throw new InvalidOperationException($"A tenant named '{dto.Name}' already exists.");

        var tenant = new Tenant
        {
            Name           = dto.Name,
            MonthlyRent    = dto.MonthlyRent,
            LeaseStartDate = dto.LeaseStartDate
        };

        await repo.AddAsync(tenant);
        await repo.SaveChangesAsync();

        return ToDetail(tenant);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var tenant = await repo.GetByIdAsync(id);
        if (tenant is null) return false;
        tenant.IsActive = false;
        await repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateAsync(int id)
    {
        var tenant = await repo.GetByIdAsync(id, ignoreFilter: true);
        if (tenant is null) return false;
        tenant.IsActive = true;
        await repo.SaveChangesAsync();
        return true;
    }
}
