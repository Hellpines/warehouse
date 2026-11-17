using WarehouseApi.DTOs;

namespace WarehouseApi.Services;

public interface IDeliveryService
{
    Task<PagedResult<DeliveryDto>> GetAsync(QueryParams query, CancellationToken ct = default);
    Task<DeliveryDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DeliveryDto> CreateAsync(DeliveryCreateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
