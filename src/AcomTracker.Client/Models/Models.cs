namespace AcomTracker.Client.Models;

public record TenantSummaryDto(
    int TenantId,
    string Name,
    decimal MonthlyRent,
    decimal TotalPaid,
    decimal TotalCharged,
    decimal OutstandingBalance,
    DateOnly? LastPayment
);

public record TenantListDto(
    int TenantId,
    string Name,
    decimal MonthlyRent,
    DateOnly LeaseStartDate,
    bool IsActive
);

public record TenantDetailDto(
    int TenantId,
    string Name,
    decimal MonthlyRent,
    DateOnly LeaseStartDate,
    decimal TotalPaid,
    decimal TotalCharged,
    decimal OutstandingBalance,
    DateOnly? LastPayment
);

public record CreateTenantDto(
    string Name,
    decimal MonthlyRent,
    DateOnly LeaseStartDate
);

public record PaymentDto(
    int PaymentId,
    int TenantId,
    DateOnly Date,
    decimal Amount,
    string Method,
    string? Notes
);

public record CreatePaymentDto(
    int TenantId,
    decimal Amount,
    DateOnly Date,
    string Method,
    string? Notes
);

public record UpdatePaymentDto(
    decimal Amount,
    DateOnly Date,
    string Method,
    string? Notes
);

public record ExpenseDto(
    int ExpenseId,
    string Description,
    decimal Amount,
    DateOnly Date,
    string? Category,
    string? Notes,
    bool HasPhoto,
    DateTime CreatedAt
);

public record ExcelPaymentRow(DateOnly Date, decimal Amount);

// Mutable classes so the Import preview table can let the user correct values
public class ExcelTenantRow
{
    public int    RowNumber             { get; set; }
    public string Name                  { get; set; } = "";
    public decimal MonthlyRent          { get; set; }
    public List<ExcelPaymentRow> Payments { get; set; } = [];
    public string? RawOutstandingBalance { get; set; }
    public List<string> Issues          { get; set; } = [];
}

public class ExcelExpenseRow
{
    public string Description   { get; set; } = "";
    public decimal Amount       { get; set; }
    public List<string> Issues  { get; set; } = [];
}

public record ImportIssue(string RowType, int RowNumber, string Field, string Message, string? RawValue = null);

public record ExcelPreviewDto(
    List<ExcelTenantRow> Tenants,
    List<ExcelExpenseRow> Expenses,
    List<ImportIssue> Issues
);

public record ConfirmImportDto(
    List<ExcelTenantRow> Tenants,
    List<ExcelExpenseRow> Expenses,
    DateOnly DefaultLeaseStart
);

public record ImportResultDto(int TenantsImported, int PaymentsImported, int ExpensesImported);
