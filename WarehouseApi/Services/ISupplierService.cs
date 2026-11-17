using WarehouseApi.DTOs;

namespace WarehouseApi.Services;

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetAsync(QueryParams query, CancellationToken ct = default);
    Task<SupplierDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SupplierDto> CreateAsync(SupplierCreateDto dto, CancellationToken ct = default);
    Task<SupplierDto?> UpdateAsync(int id, SupplierUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
