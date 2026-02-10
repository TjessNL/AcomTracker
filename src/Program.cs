using src;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AcomDb>(opt => opt.UseInMemoryDatabase("Default"));
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

app.MapGet("/Dashboard", async (AcomDb db) =>
    await db.Tenants.ToListAsync());


app.MapPost("/Tenants", async (Tenant tenant, AcomDb db) =>
{
    db.Tenants.Add(tenant);
    await db.SaveChangesAsync();

    return Results.Created($"/Tenants/{tenant.TenantId}", tenant);
});

app.MapPut("/Payments/{id}", async (int id, Payment inputPayment, AcomDb db) =>
{
    var payment = await db.Payments.FindAsync(id);

    if (payment is null) return Results.NotFound();

    payment.Amount = inputPayment.Amount;
    payment.Name = inputPayment.Name;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/Tenants/{id}", async (int id, AcomDb db) =>
{
    if (await db.Tenants.FindAsync(id) is Tenant tenant)
    {
        db.Tenants.Remove(tenant);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();
