using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}