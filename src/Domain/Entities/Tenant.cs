namespace AcomTracker.Domain.Entities;

public class Tenant
{
    public int TenantId { get; set; }
    public required string Name { get; set; }
    public decimal MonthlyRent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly LeaseStartDate { get; set; }

    public ICollection<Lease> Leases { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}