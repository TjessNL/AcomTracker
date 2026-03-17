public class Expense{
    public int ExpenseId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Category { get; set; }

  
}