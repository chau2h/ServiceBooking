using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Services;

public sealed class UpdateServiceRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    [Required]
    public int DurationMinutes { get; init; }

    [Required]
    public decimal Price { get; init; }

    public bool IsActive { get; init; }
}