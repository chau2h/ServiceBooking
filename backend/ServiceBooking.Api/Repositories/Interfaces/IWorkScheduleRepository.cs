using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Repositories.Interfaces;

public interface IWorkScheduleRepository
{
    Task<List<WorkSchedule>> GetByStaffAsync(
        long staffId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsOverlapAsync(
        long staffId,
        DateOnly workDate,
        TimeOnly newStartTime,
        TimeOnly newEndTime,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        WorkSchedule schedule,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}