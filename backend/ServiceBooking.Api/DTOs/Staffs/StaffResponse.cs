namespace ServiceBooking.Api.DTOs.Staffs;

public sealed class StaffResponse
{
    public long Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}