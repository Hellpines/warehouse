using Microsoft.Extensions.Logging.Abstractions;
using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;
using WarehouseApi.Services;
using WarehouseApi.Tests.Fakes;

namespace WarehouseApi.Tests;

public class StockServiceTests
{
    [Fact]
    public async Task SetQuantityAsync_UpdatesBalance_AndMarksLowStock()
    {
        var repo = new FakeStockRepository
        {
            Entity = new StockBalance
            {
                ProductId = 3,
                Quantity = 20,
                LastUpdated = DateTime.UtcNow.AddDays(-1),
                Product = new Product
                {
                    Id = 3,
                    Name = "Клавиатура",
                    Article = "KB-1",
                    Unit = "шт",
                    MinStockLevel = 10
                }
            }
        };

        var sut = new StockService(repo, NullLogger<StockService>.Instance);
        var result = await sut.SetQuantityAsync(3, new StockAdjustDto { Quantity = 4 });

        Assert.NotNull(result);
        Assert.Equal(4, result!.Quantity);
        Assert.True(result.IsLowStock);
        Assert.Equal(1, repo.SaveCalls);
    }

    [Fact]
    public async Task SetQuantityAsync_ReturnsNull_WhenStockMissing()
    {
        var sut = new StockService(new FakeStockRepository(), NullLogger<StockService>.Instance);
        Assert.Null(await sut.SetQuantityAsync(100, new StockAdjustDto { Quantity = 1 }));
    }
}
