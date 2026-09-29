namespace ServiceBooking.Api.DTOs.Services;

public sealed class ServiceResponse
{
    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int DurationMinutes { get; init; }

    public decimal Price { get; init; }

    public bool IsActive { get; init; }
}