using System.ComponentModel.DataAnnotations;
using ServiceBooking.Api.Common.Enums;

namespace ServiceBooking.Api.DTOs.Bookings;

public sealed class UpdateBookingStatusRequest
{
    [EnumDataType(typeof(BookingStatus))]
    public BookingStatus Status { get; init; }
}