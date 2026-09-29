using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken);

    Task<Booking?> GetByIdWithDetailsAsync(
        long id,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken);

    Task<bool> ExistsConflictAsync(
        long staffId,
        DateTime newStartTime,
        DateTime newEndTime,
        CancellationToken cancellationToken);

    Task AddAsync(
        Booking booking,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);

    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetMyBookingsAsync(
        long customerId,
        DateOnly? date,
        BookingStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetPagedAsync(
        DateOnly? date,
        BookingStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Booking>> GetNonCancelledForStaffInRangeAsync(
        long staffId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken);
}