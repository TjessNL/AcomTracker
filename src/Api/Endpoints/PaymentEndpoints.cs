namespace AcomTracker.Api.Endpoints;

using AcomTracker.Application.DTOs;
using AcomTracker.Application.Services;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        app.MapPost("/Payments", async (CreatePaymentDto dto, IPaymentService paymentService) =>
        {
            try
            {
                var created = await paymentService.CreateAsync(dto);
                return Results.Created($"/Payments/{created.PaymentId}", created);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        })
        .WithName("CreatePayment")
        .WithOpenApi();

        app.MapPut("/Payments/{id}", async (int id, UpdatePaymentDto dto, IPaymentService paymentService) =>
        {
            var found = await paymentService.UpdateAsync(id, dto);
            return found ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdatePayment")
        .WithOpenApi();
    }
}
