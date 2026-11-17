using System.ComponentModel.DataAnnotations;

namespace WarehouseApi.DTOs;

public record SupplierDto(
    int Id,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address);

public class SupplierCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ContactPerson { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }
}

public class SupplierUpdateDto : SupplierCreateDto;
