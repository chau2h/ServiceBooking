using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Services;

public sealed class ServiceQueryParameters
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [MaxLength(150)]
    public string? Search { get; init; }

    public bool? IsActive { get; init; }
}