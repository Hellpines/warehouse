using Microsoft.Extensions.Logging.Abstractions;
using WarehouseApi.Domain.Entities;
using WarehouseApi.DTOs;
using WarehouseApi.Services;
using WarehouseApi.Tests.Fakes;

namespace WarehouseApi.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_Throws_WhenArticleAlreadyExists()
    {
        var repo = new FakeProductRepository
        {
            ArticleHit = new Product { Id = 1, Article = "PAP-A4-80", Name = "Бумага" }
        };
        var sut = new ProductService(repo, NullLogger<ProductService>.Instance);
        var dto = new ProductCreateDto
        {
            Name = "Бумага",
            Article = "PAP-A4-80",
            Unit = "пач",
            MinStockLevel = 10
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateAsync(dto));
        Assert.Contains("уже существует", ex.Message);
        Assert.Empty(repo.Items);
    }

    [Fact]
    public async Task CreateAsync_AddsProduct_WithZeroStock()
    {
        var repo = new FakeProductRepository();
        var sut = new ProductService(repo, NullLogger<ProductService>.Instance);
        var dto = new ProductCreateDto
        {
            Name = "  Степлер  ",
            Article = " STP-01 ",
            Unit = "шт",
            Category = "Канцтовары",
            MinStockLevel = 5
        };

        var result = await sut.CreateAsync(dto);

        Assert.Equal(1, result.Id);
        Assert.Equal("Степлер", result.Name);
        Assert.Equal("STP-01", result.Article);
        Assert.Equal(0, result.Quantity);
        Assert.Single(repo.Items);
        Assert.NotNull(repo.Items[0].StockBalance);
        Assert.Equal(0, repo.Items[0].StockBalance!.Quantity);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_RespectsDeliveryUsage(bool usedInDeliveries)
    {
        var repo = new FakeProductRepository
        {
            HasDeliveryItems = usedInDeliveries,
            Items =
            {
                new Product { Id = 7, Name = "Мышь", Article = "MS-1", Unit = "шт" }
            }
        };
        var sut = new ProductService(repo, NullLogger<ProductService>.Instance);

        if (usedInDeliveries)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DeleteAsync(7));
            Assert.Single(repo.Items);
        }
        else
        {
            var ok = await sut.DeleteAsync(7);
            Assert.True(ok);
            Assert.Empty(repo.Items);
        }
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenProductMissing()
    {
        var sut = new ProductService(new FakeProductRepository(), NullLogger<ProductService>.Instance);
        Assert.False(await sut.DeleteAsync(99));
    }
}
