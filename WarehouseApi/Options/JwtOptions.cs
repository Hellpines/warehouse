using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "WarehouseApi";

    [Required]
    public string Audience { get; set; } = "WarehouseClients";

    /// <summary>
    /// Секрет подписи. Задаётся только через user-secrets / переменные окружения.
    /// </summary>
    [Required]
    [MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(5, 1440)]
    public int ExpirationMinutes { get; set; } = 120;
}
