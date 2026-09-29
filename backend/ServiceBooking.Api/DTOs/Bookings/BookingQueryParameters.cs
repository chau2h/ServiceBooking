using ServiceBooking.Api.Common.Enums;

namespace ServiceBooking.Api.DTOs.Bookings;

public class BookingQueryParameters
{
    public DateOnly? Date { get; set; }

    public BookingStatus? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}