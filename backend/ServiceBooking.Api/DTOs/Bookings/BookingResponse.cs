using ServiceBooking.Api.Common.Enums;

namespace ServiceBooking.Api.DTOs.Bookings;

public sealed class BookingResponse
{
    public long Id { get; init; }

    public string BookingCode { get; init; } = string.Empty;

    public long CustomerId { get; init; }

    public long ServiceId { get; init; }

    public string ServiceName { get; init; } = string.Empty;

    public long StaffId { get; init; }

    public string StaffName { get; init; } = string.Empty;

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public BookingStatus Status { get; init; }

    public string? CustomerNote { get; init; }

    public string? CancellationReason { get; init; }

    public DateTime CreatedAt { get; init; }
}