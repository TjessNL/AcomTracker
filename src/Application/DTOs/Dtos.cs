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
    string? Category = null
);

public record ExpenseDto(
    int ExpenseId,
    string Description,
    decimal Amount,
    DateOnly Date,
    string? Category
);