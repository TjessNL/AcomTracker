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
