using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Common.Options;
using ServiceBooking.Api.DTOs.Auth;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions)
    : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var user = await userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        // Do not reveal whether the email exists.
        // Both "user not found" and "wrong password" return
        // the same authentication error to avoid account enumeration.
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException(
                "INVALID_CREDENTIALS",
                "Invalid email or password.");
        }

        var verificationResult =
            passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException(
                "INVALID_CREDENTIALS",
                "Invalid email or password.");
        }

        var accessToken = jwtTokenService.CreateToken(user);

        return new LoginResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = _jwtOptions.ExpirationMinutes * 60,
            User = new AuthenticatedUserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };
    }
}