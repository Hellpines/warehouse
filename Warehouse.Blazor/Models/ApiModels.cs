using System.ComponentModel.DataAnnotations;

namespace Warehouse.Blazor.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Укажите логин")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль")]
    [MinLength(4, ErrorMessage = "Минимум 4 символа")]
    public string Password { get; set; } = string.Empty;
}

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string UserName, string Role, string? DisplayName);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public record ProductDto(int Id, string Name, string Article, string Unit, string? Category, decimal MinStockLevel, decimal Quantity, bool IsLowStock);

public class ProductCreateModel
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Article { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Unit { get; set; } = "шт";

    [MaxLength(100)]
    public string? Category { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinStockLevel { get; set; }
}

public record StockDto(int ProductId, string Article, string Name, decimal Quantity, string Unit, decimal MinStockLevel, bool IsLowStock, DateTime LastUpdated);

public record DeliveryItemDto(int ProductId, string? Article, string? ProductName, decimal Quantity, decimal UnitPrice);

public record DeliveryDto(int Id, int SupplierId, string? SupplierName, DateTime DeliveryDate, string DocumentNumber, string? Notes, IReadOnlyList<DeliveryItemDto> Items);

public record SupplierDto(int Id, string Name, string? ContactPerson, string? Phone, string? Email, string? Address);

public class SupplierFormModel
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class SessionInfo
{
    public string AccessToken { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}
