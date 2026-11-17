using Microsoft.AspNetCore.Mvc;
using WarehouseApi.DTOs;
using WarehouseApi.Extensions;
using WarehouseApi.Services;

namespace WarehouseApi.Endpoints;

public static class DeliveryEndpoints
{
    public static RouteGroupBuilder MapDeliveryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/deliveries")
            .WithTags("Deliveries")
            .RequireAuthorization();

        group.MapGet("/", async (
            [AsParameters] QueryParams query,
            IDeliveryService service,
            CancellationToken ct) =>
        {
            var result = await service.GetAsync(query, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetDeliveries")
        .WithOpenApi();

        group.MapGet("/{id:int}", async (int id, IDeliveryService service, CancellationToken ct) =>
        {
            var delivery = await service.GetByIdAsync(id, ct);
            return delivery is null ? Results.NotFound() : TypedResults.Ok(delivery);
        })
        .WithName("GetDeliveryById")
        .WithOpenApi();

        group.MapPost("/", async (DeliveryCreateDto dto, IDeliveryService service, CancellationToken ct) =>
        {
            if (!ValidationExtensions.TryValidate(dto, out var problem))
            {
                return Results.ValidationProblem(problem!.Errors);
            }

            var created = await service.CreateAsync(dto, ct);
            return TypedResults.Created($"/api/deliveries/{created.Id}", created);
        })
        .RequireAuthorization("WarehouseStaff")
        .WithName("CreateDelivery")
        .WithOpenApi();

        group.MapDelete("/{id:int}", async (int id, IDeliveryService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("ManagerOnly")
        .WithName("DeleteDelivery")
        .WithOpenApi();

        return group;
    }
}
