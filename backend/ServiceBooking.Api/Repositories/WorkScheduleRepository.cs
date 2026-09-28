using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;

namespace ServiceBooking.Api.Repositories;

public class WorkScheduleRepository(AppDbContext context)
    : IWorkScheduleRepository
{
    public async Task<List<WorkSchedule>> GetByStaffAndDateAsync(
        long staffId,
        DateOnly workDate,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<WorkSchedule>()
            .Where(schedule =>
                schedule.StaffId == staffId &&
                schedule.WorkDate == workDate)
            .OrderBy(schedule => schedule.StartTime)
            .ToListAsync(cancellationToken);
    }
}