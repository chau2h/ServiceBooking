namespace ServiceBooking.Api.DTOs.Auth;

public sealed class AuthenticatedUserResponse
{
    public long Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}