using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;

namespace WarehouseApi.Repositories;

public interface IDeliveryRepository
{
    Task<PagedResult<Delivery>> GetPagedAsync(QueryParams query, CancellationToken ct = default);
    Task<Delivery?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Delivery delivery, CancellationToken ct = default);
    Task DeleteAsync(Delivery delivery, CancellationToken ct = default);
}
