using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Укажите имя пользователя")]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль")]
    [MinLength(4)]
    public string Password { get; set; } = string.Empty;
}

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserName,
    string Role,
    string? DisplayName);
