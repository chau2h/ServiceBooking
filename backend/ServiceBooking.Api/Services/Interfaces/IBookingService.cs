using ServiceBooking.Api.Common.Enums;
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

    Task<BookingResponse> GetBookingAsync(
        long id,
        long? customerId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AvailableSlotResponse>> GetAvailableSlotsAsync(
        AvailableSlotsQueryParameters parameters,
        CancellationToken cancellationToken);

    Task<PagedResult<BookingResponse>> GetBookingsAsync(
        BookingQueryParameters parameters,
        CancellationToken cancellationToken);

    Task CancelBookingAsync(
        long id,
        long? customerId,
        string cancellationReason,
        CancellationToken cancellationToken);

    Task UpdateBookingStatusAsync(
        long id,
        BookingStatus status,
        CancellationToken cancellationToken);
}