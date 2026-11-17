using Microsoft.AspNetCore.Mvc;
using WarehouseApi.DTOs;
using WarehouseApi.Extensions;
using WarehouseApi.Services;

namespace WarehouseApi.Endpoints;

public static class StockEndpoints
{
    public static RouteGroupBuilder MapStockEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/stock")
            .WithTags("Stock")
            .RequireAuthorization();

        group.MapGet("/", async (
            [AsParameters] QueryParams query,
            bool? lowOnly,
            IStockService service,
            CancellationToken ct) =>
        {
            var result = await service.GetAsync(query, lowOnly, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetStock");

        group.MapGet("/{productId:int}", async (int productId, IStockService service, CancellationToken ct) =>
        {
            var stock = await service.GetByProductIdAsync(productId, ct);
            return stock is null ? Results.NotFound() : TypedResults.Ok(stock);
        })
        .WithName("GetStockByProduct");

        group.MapPut("/{productId:int}", async (
            int productId,
            StockAdjustDto dto,
            IStockService service,
            CancellationToken ct) =>
        {
            if (!ValidationExtensions.TryValidate(dto, out var problem))
            {
                return Results.ValidationProblem(problem!.Errors);
            }

            var updated = await service.SetQuantityAsync(productId, dto, ct);
            return updated is null ? Results.NotFound() : TypedResults.Ok(updated);
        })
        .RequireAuthorization("WarehouseStaff")
        .WithName("SetStockQuantity");

        return group;
    }
}
