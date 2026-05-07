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

    private static string BuildUrl(string path, string? search) =>
        search is { Length: > 0 }
            ? $"{path}?search={Uri.EscapeDataString(search)}"
            : path;
}
