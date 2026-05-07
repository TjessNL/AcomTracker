namespace AcomTracker.Application.DTOs;

using System.ComponentModel.DataAnnotations;

public record CreateTenantDto(
    [Required, MinLength(2)] string Name,
    [Range(1, double.MaxValue, ErrorMessage = "Monthly rent must be greater than zero.")]
    decimal MonthlyRent,
    DateOnly LeaseStartDate
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

public record TenantSummaryDto(
    int TenantId,
    string Name,
    decimal MonthlyRent,
    decimal TotalPaid,
    decimal TotalCharged,
    decimal OutstandingBalance,
    DateOnly? LastPayment
);


public record CreatePaymentDto(
    int TenantId,
    decimal Amount,
    DateOnly Date,
    string Method,
    string? Notes = null
);

public record UpdatePaymentDto(
    decimal Amount,
    DateOnly Date,
    string Method,
    string? Notes = null
);

public record PaymentDto(
    int PaymentId,
    int TenantId,
    DateOnly Date,
    decimal Amount,
    string Method,
    string? Notes
);

public record CreateExpenseDto(
    [Required] string Description,
    [Range(0.01, double.MaxValue)] decimal Amount,
    DateOnly Date,
    string? Category = null,
    string? Notes = null
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

// ── Excel Import ────────────────────────────────────────────
public record ExcelPaymentRow(DateOnly Date, decimal Amount);

public record ExcelTenantRow(
    int RowNumber,
    string Name,
    decimal MonthlyRent,
    List<ExcelPaymentRow> Payments,
    string? RawOutstandingBalance,
    List<string> Issues
);

public record ExcelExpenseRow(
    string Description,
    decimal Amount,
    List<string> Issues
);

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