namespace AcomTracker.Domain.Entities;

public class Lease
{
    public int LeaseId { get; set; }

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public decimal MonthlyAmount { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public string? UnitDescription { get; set; }
}