using Microsoft.AspNetCore.Mvc;
using WarehouseApi.DTOs;
using WarehouseApi.Extensions;
using WarehouseApi.Services;

namespace WarehouseApi.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        group.MapGet("/", async (
            [AsParameters] QueryParams query,
            IProductService service,
            CancellationToken ct) =>
        {
            var result = await service.GetAsync(query, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetProducts");

        group.MapGet("/{id:int}", async (int id, IProductService service, CancellationToken ct) =>
        {
            var product = await service.GetByIdAsync(id, ct);
            return product is null ? Results.NotFound() : TypedResults.Ok(product);
        })
        .WithName("GetProductById");

        group.MapPost("/", async (ProductCreateDto dto, IProductService service, CancellationToken ct) =>
        {
            if (!ValidationExtensions.TryValidate(dto, out var problem))
            {
                return Results.ValidationProblem(problem!.Errors);
            }

            var created = await service.CreateAsync(dto, ct);
            return TypedResults.Created($"/api/products/{created.Id}", created);
        })
        .RequireAuthorization("ManagerOnly")
        .WithName("CreateProduct");

        group.MapPut("/{id:int}", async (int id, ProductUpdateDto dto, IProductService service, CancellationToken ct) =>
        {
            if (!ValidationExtensions.TryValidate(dto, out var problem))
            {
                return Results.ValidationProblem(problem!.Errors);
            }

            var updated = await service.UpdateAsync(id, dto, ct);
            return updated is null ? Results.NotFound() : TypedResults.Ok(updated);
        })
        .RequireAuthorization("ManagerOnly")
        .WithName("UpdateProduct");

        group.MapDelete("/{id:int}", async (int id, IProductService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("ManagerOnly")
        .WithName("DeleteProduct");

        return group;
    }
}
