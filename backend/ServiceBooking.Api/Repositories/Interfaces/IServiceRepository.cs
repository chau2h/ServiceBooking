using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Repositories.Interfaces;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Service>> GetPagedAsync(
        string? search,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}