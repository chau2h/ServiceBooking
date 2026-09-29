using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Common.Helpers;
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

    public async Task<Booking?> GetByIdWithDetailsAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return await context.Set<Booking>()
            .AsNoTracking()
            .Include(booking => booking.Service)
            .Include(booking => booking.Staff)
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

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetMyBookingsAsync(
        long customerId,
        DateOnly? date,
        BookingStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Set<Booking>()
            .AsNoTracking()
            .Include(booking => booking.Service)
            .Include(booking => booking.Staff)
            .Where(booking => booking.CustomerId == customerId);

        if (date.HasValue)
        {
            var (startUtc, endUtc) =
                BusinessTime.GetUtcRangeForBusinessDate(date.Value);

            query = query.Where(
                booking =>
                    booking.StartTime >= startUtc &&
                    booking.StartTime < endUtc);
        }

        if (status.HasValue)
        {
            query = query.Where(
                booking => booking.Status == status.Value);
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var items = await query
            .OrderByDescending(booking => booking.CreatedAt)
            .ThenByDescending(booking => booking.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetPagedAsync(
        DateOnly? date,
        BookingStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Set<Booking>()
            .AsNoTracking()
            .Include(booking => booking.Service)
            .Include(booking => booking.Staff)
            .AsQueryable();

        if (date.HasValue)
        {
            var (startUtc, endUtc) =
                BusinessTime.GetUtcRangeForBusinessDate(date.Value);

            query = query.Where(
                booking => booking.StartTime >= startUtc &&
                           booking.StartTime < endUtc);
        }

        if (status.HasValue)
        {
            query = query.Where(
                booking => booking.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(booking => booking.StartTime)
            .ThenByDescending(booking => booking.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Booking>> GetNonCancelledForStaffInRangeAsync(
        long staffId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        return await context.Set<Booking>()
            .AsNoTracking()
            .Where(booking =>
                booking.StaffId == staffId &&
                booking.Status != BookingStatus.Cancelled &&
                booking.StartTime < endUtc &&
                booking.EndTime > startUtc)
            .OrderBy(booking => booking.StartTime)
            .ThenBy(booking => booking.Id)
            .ToListAsync(cancellationToken);
    }
}