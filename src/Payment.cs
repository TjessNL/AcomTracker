public class Payment
{
    public int PaymentId { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public string? Notes { get; set; }
}
