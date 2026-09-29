using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;

namespace ServiceBooking.Api.Repositories;

public class WorkScheduleRepository(AppDbContext context)
    : IWorkScheduleRepository
{
    public async Task<List<WorkSchedule>> GetByStaffAsync(
        long staffId,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<WorkSchedule>()
            .AsNoTracking()
            .Where(schedule =>
                schedule.StaffId == staffId)
            .OrderBy(schedule => schedule.WorkDate)
            .ThenBy(schedule => schedule.StartTime)
            .ThenBy(schedule => schedule.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsOverlapAsync(
        long staffId,
        DateOnly workDate,
        TimeOnly newStartTime,
        TimeOnly newEndTime,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<WorkSchedule>()
            .AnyAsync(
                schedule =>
                    schedule.StaffId == staffId &&
                    schedule.WorkDate == workDate &&
                    newStartTime < schedule.EndTime &&
                    newEndTime > schedule.StartTime,
                cancellationToken);
    }

    public async Task AddAsync(
        WorkSchedule schedule,
        CancellationToken cancellationToken = default)
    {
        await context.Set<WorkSchedule>()
            .AddAsync(schedule, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}