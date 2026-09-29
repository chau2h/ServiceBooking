using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Repositories.Interfaces;

public interface IStaffRepository
{
    Task<Staff?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<Staff?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Staff>> GetAllAsync(
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Staff staff,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}