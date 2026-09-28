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
}