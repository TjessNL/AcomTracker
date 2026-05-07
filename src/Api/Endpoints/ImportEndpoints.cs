namespace AcomTracker.Api.Endpoints;

using AcomTracker.Application.DTOs;
using AcomTracker.Application.Services;
using System.Text.Json;

public static class ImportEndpoints
{
    public static void MapImportEndpoints(this WebApplication app)
    {
        // Upload an Excel file and get a preview with detected issues
        app.MapPost("/Import/preview", async (HttpRequest request, IExcelImportService svc) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Multipart form required");

            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
                return Results.BadRequest("No file uploaded");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return Results.BadRequest("Only .xlsx and .xls files are supported");

            using var stream = file.OpenReadStream();
            var preview = svc.Preview(stream);
            return Results.Ok(preview);
        })
        .WithName("PreviewImport").WithOpenApi()
        .DisableAntiforgery();

        // Confirm the import with optionally corrected data
        app.MapPost("/Import/confirm", async (ConfirmImportDto dto, IExcelImportService svc) =>
        {
            var result = await svc.ConfirmAsync(dto);
            return Results.Ok(result);
        })
        .WithName("ConfirmImport").WithOpenApi();
    }
}
