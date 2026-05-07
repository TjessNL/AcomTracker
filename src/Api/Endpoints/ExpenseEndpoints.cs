namespace AcomTracker.Api.Endpoints;

using AcomTracker.Application.DTOs;
using AcomTracker.Application.Services;
using System.ComponentModel.DataAnnotations;

public static class ExpenseEndpoints
{
    public static void MapExpenseEndpoints(this WebApplication app)
    {
        app.MapGet("/Expenses", async (string? category, IExpenseService svc) =>
            Results.Ok(await svc.GetAllAsync(category)))
            .WithName("GetExpenses").WithOpenApi();

        app.MapGet("/Expenses/categories", async (IExpenseService svc) =>
            Results.Ok(await svc.GetCategoriesAsync()))
            .WithName("GetExpenseCategories").WithOpenApi();

        app.MapGet("/Expenses/{id}", async (int id, IExpenseService svc) =>
        {
            var e = await svc.GetByIdAsync(id);
            return e is null ? Results.NotFound() : Results.Ok(e);
        })
        .WithName("GetExpenseById").WithOpenApi();

        app.MapGet("/Expenses/{id}/photo", async (int id, IExpenseService svc) =>
        {
            var photo = await svc.GetPhotoAsync(id);
            return photo is null
                ? Results.NotFound()
                : Results.File(photo.Value.Data, photo.Value.MimeType);
        })
        .WithName("GetExpensePhoto").WithOpenApi();

        // Multipart form upload (description, amount, date, category, notes, photo file)
        app.MapPost("/Expenses", async (HttpRequest request, IExpenseService svc) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Multipart form required");

            var form = await request.ReadFormAsync();

            var description = form["description"].ToString();
            if (string.IsNullOrWhiteSpace(description))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["description"] = ["Description is required"] });

            if (!decimal.TryParse(form["amount"], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["amount"] = ["Amount must be a positive number"] });

            if (!DateOnly.TryParse(form["date"], out var date))
                date = DateOnly.FromDateTime(DateTime.Today);

            var category = form["category"].ToString().Trim().NullIfEmpty();
            var notes    = form["notes"].ToString().Trim().NullIfEmpty();

            byte[]? photoData     = null;
            string? photoMimeType = null;
            var photoFile = form.Files.GetFile("photo");
            if (photoFile is not null && photoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await photoFile.CopyToAsync(ms);
                photoData     = ms.ToArray();
                photoMimeType = photoFile.ContentType;
            }

            var dto     = new CreateExpenseDto(description, amount, date, category, notes);
            var created = await svc.CreateAsync(dto, photoData, photoMimeType);
            return Results.Created($"/Expenses/{created.ExpenseId}", created);
        })
        .WithName("CreateExpense").WithOpenApi()
        .DisableAntiforgery();

        app.MapDelete("/Expenses/{id}", async (int id, IExpenseService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteExpense").WithOpenApi();
    }

    private static string? NullIfEmpty(this string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
