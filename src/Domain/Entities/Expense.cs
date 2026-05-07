namespace AcomTracker.Domain.Entities;

public class Expense
{
    public int ExpenseId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public byte[]? PhotoData { get; set; }
    public string? PhotoMimeType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
