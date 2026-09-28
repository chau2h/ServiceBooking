using Microsoft.EntityFrameworkCore;
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
}