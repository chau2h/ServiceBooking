using ServiceBooking.Api.DTOs.Staffs;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public sealed class StaffService(
    IStaffRepository staffRepository)
: IStaffService
{
    public async Task<IReadOnlyList<StaffResponse>> GetStaffsAsync(
        StaffQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var staffs = await staffRepository.GetAllAsync(
            query.IsActive,
            cancellationToken);

        return staffs
            .Select(MapToResponse)
            .ToList();
    }

    private static StaffResponse MapToResponse(
        Models.Staff staff)
    {
        return new StaffResponse
        {
            Id = staff.Id,
            FullName = staff.FullName,
            Email = staff.Email,
            IsActive = staff.IsActive
        };
    }
}