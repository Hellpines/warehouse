using WarehouseApi.DTOs;

namespace WarehouseApi.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAsync(QueryParams query, CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(ProductCreateDto dto, CancellationToken ct = default);
    Task<ProductDto?> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
