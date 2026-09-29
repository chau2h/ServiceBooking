using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;

namespace ServiceBooking.Api.Repositories;

public sealed class BookingRepository(
    AppDbContext context)
    : IBookingRepository
{
    public async Task<Booking?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Booking>()
            .FirstOrDefaultAsync(
                booking => booking.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Booking>()
            .AnyAsync(
                booking => booking.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExistsConflictAsync(
        long staffId,
        DateTime newStartTime,
        DateTime newEndTime,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Booking>()
            .AnyAsync(
                booking =>
                    booking.StaffId == staffId &&
                    booking.Status != Common.Enums.BookingStatus.Cancelled &&

                    // Rule 5.3:
                    //
                    // NewStart < ExistingEnd
                    // AND
                    // NewEnd > ExistingStart
                    newStartTime < booking.EndTime &&
                    newEndTime > booking.StartTime,
                cancellationToken);
    }

    public async Task AddAsync(
        Booking booking,
        CancellationToken cancellationToken = default)
    {
        await context.Set<Booking>()
            .AddAsync(booking, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}