using src;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AcomDb>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("AcomDb")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>

{
    config.DocumentName = "AcomTracker";
    config.Title = "Acom notes v1";
    config.Version = "v1";
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "AcomTracker";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/Dashboard", async (string? search, AcomDb db) =>
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var query = db.Tenants.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(t => t.Name.ToLower().Contains(search.ToLower()));

    var dashboard = await query
        .Where(t => t.IsActive)
        .Select(t => new
        {
            t.TenantId,
            t.Name,
            t.MonthlyRent,
            TotalPaid      = t.Payments.Sum(p => p.Amount),
            MonthsElapsed  = (today.Year - t.LeaseStartDate.Year) * 12
                           + (today.Month - t.LeaseStartDate.Month) + 1,
            LastPayment    = t.Payments
                              .OrderByDescending(p => p.Date)
                              .Select(p => p.Date)
                              .FirstOrDefault()
        })
        .Select(t => new
        {
            t.TenantId,
            t.Name,
            t.MonthlyRent,
            t.TotalPaid,
            t.LastPayment,
            TotalCharged       = t.MonthlyRent * t.MonthsElapsed,
            OutstandingBalance = t.MonthlyRent * t.MonthsElapsed - t.TotalPaid
        })
        .OrderByDescending(t => t.OutstandingBalance)
        .ToListAsync();

    return Results.Ok(dashboard);
});


app.MapPost("/Tenants", async (CreateTenantDto dto, AcomDb db) =>
{
    var results = new List<ValidationResult>();
    var context = new ValidationContext(dto);

    if (!Validator.TryValidateObject(dto, context, results, validateAllProperties: true))
    {
        var errors = results.ToDictionary(
            r => r.MemberNames.FirstOrDefault() ?? "error",
            r => r.ErrorMessage ?? "Invalid value"
        );
        return Results.ValidationProblem(errors);
    }

    var duplicate = await db.Tenants
        .AnyAsync(t => t.Name.ToLower() == dto.Name.ToLower());

    if (duplicate)
        return Results.Conflict($"A tenant named '{dto.Name}' already exists.");

    var tenant = new Tenant
    {
        Name           = dto.Name,
        MonthlyRent    = dto.MonthlyRent,
        LeaseStartDate = dto.LeaseStartDate,
        IsActive       = true
    };

    db.Tenants.Add(tenant);
    await db.SaveChangesAsync();

    return Results.Created($"/Tenants/{tenant.TenantId}", tenant);
});

app.MapPut("/Payments/{id}", async (int id, UpdatePaymentDto dto, AcomDb db) =>
{
    var payment = await db.Payments.FindAsync(id);

    if (payment is null) return Results.NotFound();

   
    payment.Amount = dto.Amount;
    payment.Date = dto.Date;
    payment.Method = dto.Method;

    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapGet("/Tenants/{id}", async (int id, AcomDb db) =>
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var tenant = await db.Tenants
        .Where(t => t.TenantId == id)
        .Select(t => new
        {
            t.TenantId,
            t.Name,
            t.MonthlyRent,
            t.LeaseStartDate,
            TotalPaid     = t.Payments.Sum(p => p.Amount),
            LastPayment   = t.Payments
                             .OrderByDescending(p => p.Date)
                             .Select(p => p.Date)
                             .FirstOrDefault(),
            MonthsElapsed = (today.Year - t.LeaseStartDate.Year) * 12
                          + (today.Month - t.LeaseStartDate.Month) + 1
        })
        .Select(t => new
        {
            t.TenantId,
            t.Name,
            t.MonthlyRent,
            t.LeaseStartDate,
            t.TotalPaid,
            t.LastPayment,
            TotalCharged       = t.MonthlyRent * t.MonthsElapsed,
            OutstandingBalance = t.MonthlyRent * t.MonthsElapsed - t.TotalPaid
        })
        .FirstOrDefaultAsync();

    return tenant is null ? Results.NotFound() : Results.Ok(tenant);
});
app.MapGet("/Tenants", async (string? search, AcomDb db) =>
{
    var query = db.Tenants.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(t => t.Name.ToLower().Contains(search.ToLower()));

    var tenants = await query
        .OrderBy(t => t.Name)
        .Select(t => new
        {
            t.TenantId,
            t.Name,
            t.MonthlyRent,
            t.LeaseStartDate,
            t.IsActive
        })
        .ToListAsync();

    return Results.Ok(tenants);
});
app.MapGet("/Tenants/{id}/payments", async (int id, AcomDb db) =>
{
    var tenant = await db.Tenants.FindAsync(id);
    if (tenant is null) return Results.NotFound($"Tenant {id} not found.");

    var payments = await db.Payments
        .Where(p => p.TenantId == id)
        .OrderByDescending(p => p.Date)
        .Select(p => new
        {
            p.PaymentId,
            p.Date,
            p.Amount,
            p.Method,
            p.Notes
        })
        .ToListAsync();

    return Results.Ok(payments);
});

app.MapPost("/Payments", async (CreatePaymentDto dto, AcomDb db) =>
{
    var tenant = await db.Tenants.FindAsync(dto.TenantId);
    if (tenant is null) return Results.NotFound($"Tenant {dto.TenantId} not found.");

    var payment = new Payment
    {
        TenantId = dto.TenantId,
        Amount   = dto.Amount,
        Date     = dto.Date,
        Method   = dto.Method
    };

    db.Payments.Add(payment);
    await db.SaveChangesAsync();

    return Results.Created($"/Payments/{payment.PaymentId}", payment);
});

app.MapDelete("/Tenants/{id}", async (int id, AcomDb db) =>
{
    var tenant = await db.Tenants.FindAsync(id);

    if (tenant is null) return Results.NotFound();

    if (!tenant.IsActive)
        return Results.Conflict($"Tenant {id} is already inactive.");

    tenant.IsActive = false;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPut("/Tenants/{id}/reactivate", async (int id, AcomDb db) =>
{
    var tenant = await db.Tenants.FindAsync(id);

    if (tenant is null) return Results.NotFound();
    if (tenant.IsActive) return Results.Conflict($"Tenant {id} is already active.");

    tenant.IsActive = true;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
public record UpdatePaymentDto(decimal Amount, DateOnly Date, string Method);
public record CreatePaymentDto(int TenantId, decimal Amount, DateOnly Date, string Method);
public record CreateTenantDto(
    [Required, MinLength(2)] string Name,
    [Range(1, double.MaxValue, ErrorMessage = "Monthly rent must be greater than zero.")] decimal MonthlyRent,
    DateOnly LeaseStartDate
);