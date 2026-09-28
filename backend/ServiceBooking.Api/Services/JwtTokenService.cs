using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceBooking.Api.Common.Options;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public sealed class JwtTokenService(
    IOptions<JwtOptions> jwtOptions)
    : IJwtTokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public string CreateToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException(
                "JWT secret key is not configured.");
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                ClaimTypes.Name,
                user.FullName),

            new(
                ClaimTypes.Role,
                user.Role.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(
                _options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}