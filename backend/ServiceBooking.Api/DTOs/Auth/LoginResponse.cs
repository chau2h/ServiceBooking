namespace ServiceBooking.Api.DTOs.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public string TokenType { get; init; } = "Bearer";

    public int ExpiresIn { get; init; }

    public AuthenticatedUserResponse User { get; init; } = null!;
}