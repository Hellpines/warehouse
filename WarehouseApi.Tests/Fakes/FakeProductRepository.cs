using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;
using WarehouseApi.Repositories;

namespace WarehouseApi.Tests.Fakes;

public class FakeProductRepository : IProductRepository
{
    public List<Product> Items { get; } = [];
    public bool HasDeliveryItems { get; set; }
    public Product? ArticleHit { get; set; }

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        if (product.Id == 0) product.Id = Items.Count == 0 ? 1 : Items.Max(x => x.Id) + 1;
        Items.Add(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        Items.Remove(product);
        return Task.CompletedTask;
    }

    public Task<Product?> GetByArticleAsync(string article, CancellationToken ct = default) =>
        Task.FromResult(ArticleHit ?? Items.FirstOrDefault(x => x.Article == article));

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

    public Task<PagedResult<Product>> GetPagedAsync(QueryParams query, CancellationToken ct = default)
    {
        var list = Items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            list = list.Where(p => p.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        }

        var arr = list.ToList();
        return Task.FromResult(new PagedResult<Product>(arr, 1, arr.Count, arr.Count, 1));
    }

    public Task<bool> HasDeliveryItemsAsync(int productId, CancellationToken ct = default) =>
        Task.FromResult(HasDeliveryItems);

    public Task UpdateAsync(Product product, CancellationToken ct = default) => Task.CompletedTask;
}
