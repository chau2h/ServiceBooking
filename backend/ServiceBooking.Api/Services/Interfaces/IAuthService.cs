using ServiceBooking.Api.DTOs.Auth;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
    
    Task<AuthenticatedUserResponse> GetCurrentUserAsync(
        long userId,
        CancellationToken cancellationToken = default);
}