using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Bookings;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(
        long customerId,
        CreateBookingRequest request,
        CancellationToken cancellationToken);

    Task<PagedResult<BookingResponse>> GetMyBookingsAsync(
        long customerId,
        BookingQueryParameters parameters,
        CancellationToken cancellationToken);
}