using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;
using WarehouseApi.Mapping;
using WarehouseApi.Repositories;

namespace WarehouseApi.Services;

public class ProductService(IProductRepository products, ILogger<ProductService> logger) : IProductService
{
    public async Task<PagedResult<ProductDto>> GetAsync(QueryParams query, CancellationToken ct = default)
    {
        var page = await products.GetPagedAsync(query, ct);
        var items = page.Items.Select(EntityMapper.ToDto).ToList();
        return new PagedResult<ProductDto>(items, page.Page, page.PageSize, page.TotalCount, page.TotalPages);
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await products.GetByIdAsync(id, ct);
        return product is null ? null : EntityMapper.ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto, CancellationToken ct = default)
    {
        var existing = await products.GetByArticleAsync(dto.Article, ct);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Товар с артикулом '{dto.Article}' уже существует.");
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Article = dto.Article.Trim(),
            Unit = dto.Unit.Trim(),
            Category = dto.Category?.Trim(),
            MinStockLevel = dto.MinStockLevel,
            StockBalance = new StockBalance
            {
                Quantity = 0,
                LastUpdated = DateTime.UtcNow
            }
        };

        await products.AddAsync(product, ct);
        logger.LogInformation("Создан товар {Article} (Id={Id})", product.Article, product.Id);
        return EntityMapper.ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default)
    {
        var product = await products.GetByIdAsync(id, ct);
        if (product is null) return null;

        product.Name = dto.Name.Trim();
        product.Unit = dto.Unit.Trim();
        product.Category = dto.Category?.Trim();
        product.MinStockLevel = dto.MinStockLevel;

        await products.UpdateAsync(product, ct);
        logger.LogInformation("Обновлён товар Id={Id}", id);
        return EntityMapper.ToDto(product);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await products.GetByIdAsync(id, ct);
        if (product is null) return false;

        if (await products.HasDeliveryItemsAsync(id, ct))
        {
            throw new InvalidOperationException("Нельзя удалить товар, участвующий в поставках.");
        }

        await products.DeleteAsync(product, ct);
        logger.LogInformation("Удалён товар Id={Id}", id);
        return true;
    }
}
