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

    Task<IReadOnlyList<Staff>> GetAllAsync(
        bool? isActive,
        CancellationToken cancellationToken = default);
}