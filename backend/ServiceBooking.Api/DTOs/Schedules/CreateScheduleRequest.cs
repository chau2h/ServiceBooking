using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Schedules;

public sealed class CreateScheduleRequest
{
    [Required]
    public DateOnly WorkDate { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }
}