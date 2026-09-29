namespace ServiceBooking.Api.DTOs.Bookings;

public sealed class AvailableSlotResponse
{
    public DateTimeOffset StartTime { get; init; }

    public DateTimeOffset EndTime { get; init; }
}