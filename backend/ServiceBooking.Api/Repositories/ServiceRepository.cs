using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;

namespace ServiceBooking.Api.Repositories;

public class ServiceRepository(AppDbContext context) : IServiceRepository
{
    public async Task<Service?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Service>()
            .FirstOrDefaultAsync(
                service => service.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Service>()
            .AnyAsync(
                service => service.Id == id,
                cancellationToken);
    }

    public async Task<PagedResult<Service>> GetPagedAsync(
        string? search,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Service> query =
            context.Set<Service>()
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();

            // PostgreSQL ILIKE provides case-insensitive matching.
            query = query.Where(service =>
                EF.Functions.ILike(
                    service.Name,
                    $"%{normalizedSearch}%"));
        }

        if (isActive.HasValue)
        {
            query = query.Where(service =>
                service.IsActive == isActive.Value);
        }

        // Count is executed by PostgreSQL.
        // We do NOT load all rows into memory before pagination.
        var totalItems = await query.CountAsync(
            cancellationToken);

        var totalPages = (int)Math.Ceiling(
            totalItems / (double)pageSize);

        var services = await query
            // Stable ordering is important for reliable pagination.
            .OrderBy(service => service.Name)
            .ThenBy(service => service.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Service>
        {
            Items = services,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}