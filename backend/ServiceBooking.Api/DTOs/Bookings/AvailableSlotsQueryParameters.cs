using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Bookings;

public sealed class AvailableSlotsQueryParameters
{
    [Range(1, long.MaxValue)]
    public long ServiceId { get; init; }

    [Range(1, long.MaxValue)]
    public long StaffId { get; init; }

    public DateOnly Date { get; init; }
}