using Microsoft.AspNetCore.Mvc;
using WarehouseApi.DTOs;
using WarehouseApi.Extensions;
using WarehouseApi.Services;

namespace WarehouseApi.Endpoints;

public static class SupplierEndpoints
{
    public static RouteGroupBuilder MapSupplierEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/suppliers")
            .WithTags("Suppliers")
            .RequireAuthorization();

        group.MapGet("/", async (
            [AsParameters] QueryParams query,
            ISupplierService service,
            CancellationToken ct) =>
        {
            var result = await service.GetAsync(query, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetSuppliers")
        .WithOpenApi();

        group.MapGet("/{id:int}", async (int id, ISupplierService service, CancellationToken ct) =>
        {
            var supplier = await service.GetByIdAsync(id, ct);
            return supplier is null ? Results.NotFound() : TypedResults.Ok(supplier);
        })
        .WithName("GetSupplierById")
        .WithOpenApi();

        group.MapPost("/", async (SupplierCreateDto dto, ISupplierService service, CancellationToken ct) =>
        {
            if (!ValidationExtensions.TryValidate(dto, out var problem))
            {
                return Results.ValidationProblem(problem!.Errors);
            }

            var created = await service.CreateAsync(dto, ct);
            return TypedResults.Created($"/api/suppliers/{created.Id}", created);
        })
        .RequireAuthorization("ManagerOnly")
        .WithName("CreateSupplier")
        .WithOpenApi();

        group.MapPut("/{id:int}", async (int id, SupplierUpdateDto dto, ISupplierService service, CancellationToken ct) =>
        {
            if (!ValidationExtensions.TryValidate(dto, out var problem))
            {
                return Results.ValidationProblem(problem!.Errors);
            }

            var updated = await service.UpdateAsync(id, dto, ct);
            return updated is null ? Results.NotFound() : TypedResults.Ok(updated);
        })
        .RequireAuthorization("ManagerOnly")
        .WithName("UpdateSupplier")
        .WithOpenApi();

        group.MapDelete("/{id:int}", async (int id, ISupplierService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("ManagerOnly")
        .WithName("DeleteSupplier")
        .WithOpenApi();

        return group;
    }
}
