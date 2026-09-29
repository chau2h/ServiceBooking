using ServiceBooking.Api.DTOs.Schedules;
using ServiceBooking.Api.DTOs.Staffs;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IStaffService
{
    Task<IReadOnlyList<StaffResponse>> GetStaffsAsync(
        StaffQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScheduleResponse>> GetSchedulesAsync(
        long staffId,
        CancellationToken cancellationToken = default);

    Task<ScheduleResponse> CreateScheduleAsync(
        long staffId,
        CreateScheduleRequest request,
        CancellationToken cancellationToken = default);
}