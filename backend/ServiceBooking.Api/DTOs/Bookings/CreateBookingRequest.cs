using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Bookings;

public sealed class CreateBookingRequest
{
    [Required]
    public long ServiceId { get; init; }

    [Required]
    public long StaffId { get; init; }

    [Required]
    public DateTimeOffset StartTime { get; init; }

    [MaxLength(1000)]
    public string? CustomerNote { get; init; }
}