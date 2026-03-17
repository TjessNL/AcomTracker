using AcomTracker.Api.Endpoints;
using AcomTracker.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "AcomTracker";
    config.Title        = "Acom notes v1";
    config.Version      = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "AcomTracker";
        config.Path          = "/swagger";
        config.DocumentPath  = "/swagger/{documentName}/swagger.json";
        config.DocExpansion  = "list";
    });
}

app.MapDashboardEndpoints();
app.MapTenantEndpoints();
app.MapPaymentEndpoints();

app.Run();