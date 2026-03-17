namespace AcomTracker.Api.Endpoints;
 
using AcomTracker.Application.Services;
 
public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/Dashboard", async (string? search, ITenantService tenantService) =>
        {
            var summary = await tenantService.GetDashboardAsync(search);
            return Results.Ok(summary);
        })
        .WithName("GetDashboard")
        .WithOpenApi();
    }
}