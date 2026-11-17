using WarehouseApi.DTOs;
using WarehouseApi.Extensions;
using WarehouseApi.Services;

namespace WarehouseApi.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, IAuthService auth, CancellationToken ct) =>
        {
            if (!ValidationExtensions.TryValidate(request, out var problem))
            {
                return Results.ValidationProblem(problem!.Errors);
            }

            var result = await auth.LoginAsync(request, ct);
            return result is null
                ? Results.Problem(
                    detail: "Неверное имя пользователя или пароль.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized")
                : TypedResults.Ok(result);
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithOpenApi();

        return group;
    }
}
