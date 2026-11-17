using WarehouseApi.Domain.Entities;
using WarehouseApi.Mapping;

namespace WarehouseApi.Tests;

public class EntityMapperTests
{
    [Theory]
    [InlineData(5, 10, true)]
    [InlineData(10, 10, false)]
    [InlineData(15, 10, false)]
    public void ToDto_Product_ComputesIsLowStock(decimal qty, decimal min, bool expectedLow)
    {
        var product = new Product
        {
            Id = 1,
            Name = "Скотч",
            Article = "TAPE",
            Unit = "шт",
            MinStockLevel = min,
            StockBalance = new StockBalance { ProductId = 1, Quantity = qty }
        };

        var dto = EntityMapper.ToDto(product);
        Assert.Equal(expectedLow, dto.IsLowStock);
        Assert.Equal(qty, dto.Quantity);
    }
}
