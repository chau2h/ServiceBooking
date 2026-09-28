using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default);
}