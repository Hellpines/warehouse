using System.Net.Http.Json;
using Warehouse.Blazor.Models;

namespace Warehouse.Blazor.Services;

public class WarehouseApiClient(HttpClient http)
{
    public async Task<LoginResponse?> LoginAsync(LoginModel model, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", model, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
    }

    public Task<PagedResult<ProductDto>?> GetProductsAsync(string? search = null, int page = 1, CancellationToken ct = default) =>
        http.GetFromJsonAsync<PagedResult<ProductDto>>(
            $"api/products?page={page}&pageSize=50&search={Uri.EscapeDataString(search ?? string.Empty)}&sortBy=name", ct);

    public async Task<ProductDto?> CreateProductAsync(ProductCreateModel model, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/products", model, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: ct);
    }

    public async Task<bool> UpdateProductAsync(int id, ProductCreateModel model, CancellationToken ct = default)
    {
        var body = new
        {
            model.Name,
            model.Unit,
            model.Category,
            model.MinStockLevel
        };
        var response = await http.PutAsJsonAsync($"api/products/{id}", body, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken ct = default)
    {
        var response = await http.DeleteAsync($"api/products/{id}", ct);
        return response.IsSuccessStatusCode;
    }

    public Task<PagedResult<StockDto>?> GetStockAsync(bool lowOnly = false, CancellationToken ct = default) =>
        http.GetFromJsonAsync<PagedResult<StockDto>>(
            $"api/stock?page=1&pageSize=100&lowOnly={lowOnly.ToString().ToLowerInvariant()}&sortBy=name", ct);

    public async Task<StockDto?> SetStockAsync(int productId, decimal quantity, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"api/stock/{productId}", new { quantity }, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<StockDto>(cancellationToken: ct);
    }

    public Task<PagedResult<DeliveryDto>?> GetDeliveriesAsync(CancellationToken ct = default) =>
        http.GetFromJsonAsync<PagedResult<DeliveryDto>>("api/deliveries?page=1&pageSize=50&sortBy=date&desc=true", ct);

    public async Task<DeliveryDto?> CreateDeliveryAsync(object body, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/deliveries", body, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DeliveryDto>(cancellationToken: ct);
    }

    public async Task<bool> DeleteDeliveryAsync(int id, CancellationToken ct = default)
    {
        var response = await http.DeleteAsync($"api/deliveries/{id}", ct);
        return response.IsSuccessStatusCode;
    }

    public Task<PagedResult<SupplierDto>?> GetSuppliersAsync(CancellationToken ct = default) =>
        http.GetFromJsonAsync<PagedResult<SupplierDto>>("api/suppliers?page=1&pageSize=50", ct);

    public async Task<SupplierDto?> CreateSupplierAsync(SupplierDto model, CancellationToken ct = default)
    {
        var body = new
        {
            model.Name,
            model.ContactPerson,
            model.Phone,
            model.Email,
            model.Address
        };
        var response = await http.PostAsJsonAsync("api/suppliers", body, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SupplierDto>(cancellationToken: ct);
    }

    public async Task<bool> UpdateSupplierAsync(int id, SupplierDto model, CancellationToken ct = default)
    {
        var body = new
        {
            model.Name,
            model.ContactPerson,
            model.Phone,
            model.Email,
            model.Address
        };
        var response = await http.PutAsJsonAsync($"api/suppliers/{id}", body, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSupplierAsync(int id, CancellationToken ct = default)
    {
        var response = await http.DeleteAsync($"api/suppliers/{id}", ct);
        return response.IsSuccessStatusCode;
    }
}
