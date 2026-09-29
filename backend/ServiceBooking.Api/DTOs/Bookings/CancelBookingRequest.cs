using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Bookings;

public sealed class CancelBookingRequest
{
    [Required]
    [MaxLength(500)]
    public string CancellationReason { get; init; } = string.Empty;
}