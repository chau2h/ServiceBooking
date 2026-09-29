using ServiceBooking.Api.DTOs.Bookings;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(
        int customerId,
        CreateBookingRequest request,
        CancellationToken cancellationToken);
}