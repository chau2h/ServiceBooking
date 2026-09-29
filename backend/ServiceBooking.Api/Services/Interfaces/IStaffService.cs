using ServiceBooking.Api.DTOs.Staffs;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IStaffService
{
    Task<IReadOnlyList<StaffResponse>> GetStaffsAsync(
        StaffQueryParameters query,
        CancellationToken cancellationToken = default);
}