namespace ServiceBooking.Api.DTOs.Schedules;

public sealed class ScheduleResponse
{
    public long Id { get; init; }

    public long StaffId { get; init; }

    public DateOnly WorkDate { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}