public class TenantService(ITenantRepository repo) : ITenantService
{
    private static int MonthsElapsed(DateOnly start)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return (today.Year - start.Year) * 12
             + (today.Month - start.Month) + 1;
    }

    public async Task<TenantDetailDto?> GetByIdAsync(int id)
    {
        var tenant = await repo.GetByIdAsync(id);
        if (tenant is null) return null;

        var months = MonthsElapsed(tenant.LeaseStartDate);
        var totalCharged = tenant.MonthlyRent * months;
        var totalPaid = tenant.Payments.Sum(p => p.Amount);

        return new TenantDetailDto(
            tenant.TenantId, tenant.Name, tenant.MonthlyRent,
            tenant.LeaseStartDate, totalPaid, totalCharged,
            totalCharged - totalPaid,
            tenant.Payments.OrderByDescending(p => p.Date)
                           .Select(p => p.Date).FirstOrDefault()
        );
    }

}