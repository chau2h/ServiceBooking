using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;

namespace ServiceBooking.Api.Repositories;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    public async Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Booking>()
            .AnyAsync(
                booking => booking.Id == id,
                cancellationToken);
    }
}