namespace AcomTracker.Api.Endpoints;

using AcomTracker.Application.DTOs;
using AcomTracker.Application.Services;
using System.ComponentModel.DataAnnotations;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        app.MapGet("/Tenants", async (string? search, ITenantService tenantService) =>
        {
            var tenants = await tenantService.GetAllAsync(search);
            return Results.Ok(tenants);
        })
        .WithName("GetTenants")
        .WithOpenApi();

        app.MapGet("/Tenants/{id}", async (int id, ITenantService tenantService) =>
        {
            var tenant = await tenantService.GetByIdAsync(id);
            return tenant is null ? Results.NotFound() : Results.Ok(tenant);
        })
        .WithName("GetTenantById")
        .WithOpenApi();

        app.MapPost("/Tenants", async (CreateTenantDto dto, ITenantService tenantService) =>
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);

            if (!Validator.TryValidateObject(dto, context, results, validateAllProperties: true))
            {
                var errors = results.ToDictionary(
                    r => r.MemberNames.FirstOrDefault() ?? "error",
                    r => new[] { r.ErrorMessage ?? "Invalid value" }
                );
                return Results.ValidationProblem(errors);
            }

            try
            {
                var created = await tenantService.CreateAsync(dto);
                return Results.Created($"/Tenants/{created.TenantId}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(ex.Message);
            }
        })
        .WithName("CreateTenant")
        .WithOpenApi();

        app.MapDelete("/Tenants/{id}", async (int id, ITenantService tenantService) =>
        {
            var found = await tenantService.DeactivateAsync(id);
            return found ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeactivateTenant")
        .WithOpenApi();

        app.MapPut("/Tenants/{id}/reactivate", async (int id, ITenantService tenantService) =>
        {
            var found = await tenantService.ReactivateAsync(id);
            return found ? Results.NoContent() : Results.NotFound();
        })
        .WithName("ReactivateTenant")
        .WithOpenApi();

        app.MapGet("/Tenants/{id}/payments", async (int id, IPaymentService paymentService) =>
        {
            try
            {
                var payments = await paymentService.GetByTenantIdAsync(id);
                return Results.Ok(payments);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        })
        .WithName("GetTenantPayments")
        .WithOpenApi();
    }
}