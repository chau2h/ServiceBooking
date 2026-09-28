using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Repositories.Interfaces;

public interface IWorkScheduleRepository
{
    Task<List<WorkSchedule>> GetByStaffAndDateAsync(
        long staffId,
        DateOnly workDate,
        CancellationToken cancellationToken = default);
}