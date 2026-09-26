using ServiceBooking.Api.Common.Enums;

namespace ServiceBooking.Api.Models;

public class Booking
{
    public long Id { get; set; }

    public string BookingCode { get; set; } = string.Empty;

    public long CustomerId { get; set; }

    public long ServiceId { get; set; }

    public long StaffId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public BookingStatus Status { get; set; }

    public string? CustomerNote { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }

    // Foreign key navigations.

    public User Customer { get; set; } = null!;

    public Service Service { get; set; } = null!;

    public Staff Staff { get; set; } = null!;
}