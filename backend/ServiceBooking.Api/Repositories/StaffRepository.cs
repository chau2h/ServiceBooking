using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;

namespace ServiceBooking.Api.Repositories;

public class StaffRepository(AppDbContext context) : IStaffRepository
{
    public async Task<Staff?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Staff>()
            .FirstOrDefaultAsync(
                staff => staff.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Staff>()
            .AnyAsync(
                staff => staff.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Staff>> GetAllAsync(
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Staff> query = context.Set<Staff>()
            .AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(staff =>
                staff.IsActive == isActive.Value);
        }

        return await query
            // Stable ordering for a predictable API response.
            .OrderBy(staff => staff.FullName)
            .ThenBy(staff => staff.Id)
            .ToListAsync(cancellationToken);
    }
}