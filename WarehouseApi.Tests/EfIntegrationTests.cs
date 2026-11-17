using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WarehouseApi.Data;
using WarehouseApi.DTOs;
using WarehouseApi.Repositories;
using WarehouseApi.Services;

namespace WarehouseApi.Tests;

/// <summary>
/// Интеграционный тест: сервис + репозиторий + EF Core (SQLite in-memory).
/// </summary>
public class EfIntegrationTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private WarehouseDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new WarehouseDbContext(options);
        await DbSeeder.SeedAsync(_db, NullLogger.Instance);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task SeededCatalog_IsQueryable_ThroughProductService()
    {
        var service = new ProductService(new ProductRepository(_db), NullLogger<ProductService>.Instance);
        var page = await service.GetAsync(new QueryParams(Page: 1, PageSize: 20, Search: "USB"));

        Assert.True(page.TotalCount >= 1);
        Assert.Contains(page.Items, p => p.Article == "KB-USB-01");
    }

    [Fact]
    public async Task CreateProduct_Persists_AndAppearsInQuery()
    {
        var service = new ProductService(new ProductRepository(_db), NullLogger<ProductService>.Instance);
        var created = await service.CreateAsync(new ProductCreateDto
        {
            Name = "Дырокол",
            Article = "PCH-01",
            Unit = "шт",
            MinStockLevel = 3
        });

        Assert.True(created.Id > 0);
        var loaded = await service.GetByIdAsync(created.Id);
        Assert.NotNull(loaded);
        Assert.Equal("PCH-01", loaded!.Article);
        Assert.Equal(0, loaded.Quantity);
    }
}
