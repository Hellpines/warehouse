using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Warehouse.Blazor.Models;

namespace Warehouse.Blazor.Services;

public class WarehouseApiClient(HttpClient http, TokenStore tokens)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = content
        };

        if (tokens.IsAuthenticated && !string.IsNullOrWhiteSpace(tokens.Session?.AccessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokens.Session.AccessToken);
        }

        return request;
    }

    private static StringContent JsonBody(object value) =>
        new(JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8, "application/json");

    public async Task<LoginResponse?> LoginAsync(LoginModel model, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        {
            Content = JsonBody(model)
        };
        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, ct);
    }

    public async Task<PagedResult<ProductDto>?> GetProductsAsync(string? search = null, int page = 1, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Get,
            $"api/products?page={page}&pageSize=50&search={Uri.EscapeDataString(search ?? string.Empty)}&sortBy=name");
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>(JsonOptions, ct);
    }

    public async Task<ProductDto?> CreateProductAsync(ProductCreateModel model, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/products", JsonBody(model));
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions, ct);
    }

    public async Task<bool> UpdateProductAsync(int id, ProductCreateModel model, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Put, $"api/products/{id}",
            JsonBody(new { model.Name, model.Unit, model.Category, model.MinStockLevel }));
        using var response = await http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, $"api/products/{id}");
        using var response = await http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<PagedResult<StockDto>?> GetStockAsync(bool lowOnly = false, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Get,
            $"api/stock?page=1&pageSize=100&lowOnly={lowOnly.ToString().ToLowerInvariant()}&sortBy=name");
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PagedResult<StockDto>>(JsonOptions, ct);
    }

    public async Task<StockDto?> SetStockAsync(int productId, decimal quantity, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Put, $"api/stock/{productId}",
            JsonBody(new { quantity }));
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<StockDto>(JsonOptions, ct);
    }

    public async Task<PagedResult<DeliveryDto>?> GetDeliveriesAsync(CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/deliveries?page=1&pageSize=50&sortBy=date&desc=true");
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PagedResult<DeliveryDto>>(JsonOptions, ct);
    }

    public async Task<DeliveryDto?> CreateDeliveryAsync(object body, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/deliveries", JsonBody(body));
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DeliveryDto>(JsonOptions, ct);
    }

    public async Task<bool> DeleteDeliveryAsync(int id, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, $"api/deliveries/{id}");
        using var response = await http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<PagedResult<SupplierDto>?> GetSuppliersAsync(CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/suppliers?page=1&pageSize=50");
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PagedResult<SupplierDto>>(JsonOptions, ct);
    }

    public async Task<SupplierDto?> CreateSupplierAsync(SupplierDto model, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/suppliers",
            JsonBody(new { model.Name, model.ContactPerson, model.Phone, model.Email, model.Address }));
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SupplierDto>(JsonOptions, ct);
    }

    public async Task<bool> UpdateSupplierAsync(int id, SupplierDto model, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Put, $"api/suppliers/{id}",
            JsonBody(new { model.Name, model.ContactPerson, model.Phone, model.Email, model.Address }));
        using var response = await http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSupplierAsync(int id, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, $"api/suppliers/{id}");
        using var response = await http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }
}
