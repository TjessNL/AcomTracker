using System.Net.Http.Json;
using System.Text.Json;
using AcomTracker.Client.Models;

namespace AcomTracker.Client.Services;

public class AcomApiService(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<TenantSummaryDto[]?> GetDashboardAsync(string? search = null) =>
        http.GetFromJsonAsync<TenantSummaryDto[]>(
            BuildUrl("Dashboard", search), JsonOpts);

    public Task<TenantListDto[]?> GetTenantsAsync(string? search = null) =>
        http.GetFromJsonAsync<TenantListDto[]>(
            BuildUrl("Tenants", search), JsonOpts);

    public Task<TenantDetailDto?> GetTenantAsync(int id) =>
        http.GetFromJsonAsync<TenantDetailDto>($"Tenants/{id}", JsonOpts);

    public Task<PaymentDto[]?> GetPaymentsAsync(int tenantId) =>
        http.GetFromJsonAsync<PaymentDto[]>($"Tenants/{tenantId}/payments", JsonOpts);

    public async Task<TenantDetailDto?> CreateTenantAsync(CreateTenantDto dto)
    {
        var response = await http.PostAsJsonAsync("Tenants", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TenantDetailDto>(JsonOpts);
    }

    public async Task DeactivateTenantAsync(int id)
    {
        var response = await http.DeleteAsync($"Tenants/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ReactivateTenantAsync(int id)
    {
        var response = await http.PutAsync($"Tenants/{id}/reactivate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PaymentDto?> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var response = await http.PostAsJsonAsync("Payments", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaymentDto>(JsonOpts);
    }

    public async Task UpdatePaymentAsync(int id, UpdatePaymentDto dto)
    {
        var response = await http.PutAsJsonAsync($"Payments/{id}", dto);
        response.EnsureSuccessStatusCode();
    }

    // ── Expenses ──────────────────────────────────────────────

    public Task<ExpenseDto[]?> GetExpensesAsync(string? category = null) =>
        http.GetFromJsonAsync<ExpenseDto[]>(
            category is { Length: > 0 } ? $"Expenses?category={Uri.EscapeDataString(category)}" : "Expenses",
            JsonOpts);

    public Task<string[]?> GetExpenseCategoriesAsync() =>
        http.GetFromJsonAsync<string[]>("Expenses/categories", JsonOpts);

    public async Task<ExpenseDto?> CreateExpenseAsync(
        string description, decimal amount, DateOnly date,
        string? category, string? notes,
        byte[]? photoData, string? photoMimeType, string? photoFileName)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(description),                             "description");
        form.Add(new StringContent(amount.ToString(System.Globalization.CultureInfo.InvariantCulture)), "amount");
        form.Add(new StringContent(date.ToString("yyyy-MM-dd")),             "date");
        if (category is { Length: > 0 }) form.Add(new StringContent(category), "category");
        if (notes    is { Length: > 0 }) form.Add(new StringContent(notes),    "notes");
        if (photoData is not null)
        {
            var fileContent = new ByteArrayContent(photoData);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photoMimeType ?? "image/jpeg");
            form.Add(fileContent, "photo", photoFileName ?? "photo.jpg");
        }
        var response = await http.PostAsync("Expenses", form);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExpenseDto>(JsonOpts);
    }

    public async Task DeleteExpenseAsync(int id)
    {
        var response = await http.DeleteAsync($"Expenses/{id}");
        response.EnsureSuccessStatusCode();
    }

    // ── Import ────────────────────────────────────────────────

    public async Task<ExcelPreviewDto?> PreviewImportAsync(byte[] fileBytes, string fileName)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.Add(fileContent, "file", fileName);
        var response = await http.PostAsync("Import/preview", form);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExcelPreviewDto>(JsonOpts);
    }

    public async Task<ImportResultDto?> ConfirmImportAsync(ConfirmImportDto dto)
    {
        var response = await http.PostAsJsonAsync("Import/confirm", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ImportResultDto>(JsonOpts);
    }

    public string GetPhotoUrl(int expenseId) =>
        $"{http.BaseAddress}Expenses/{expenseId}/photo";

    private static string BuildUrl(string path, string? search) =>
        search is { Length: > 0 }
            ? $"{path}?search={Uri.EscapeDataString(search)}"
            : path;
}
