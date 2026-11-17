using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WarehouseApi.DTOs;
using WarehouseApi.Options;
using WarehouseApi.Repositories;

namespace WarehouseApi.Services;

public class AuthService(
    IUserRepository users,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.FindByUserNameAsync(request.UserName, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Неудачная попытка входа для пользователя {UserName}", request.UserName);
            return null;
        }

        var opts = jwtOptions.Value;
        var expires = DateTime.UtcNow.AddMinutes(opts.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role),
            new("display_name", user.DisplayName ?? user.UserName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        logger.LogInformation("Пользователь {UserName} вошёл с ролью {Role}", user.UserName, user.Role);

        return new LoginResponse(accessToken, expires, user.UserName, user.Role, user.DisplayName);
    }
}
