using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Services;

public static class BookingScheduleValidator
{
    public static bool IsWithinSchedule(
        DateTimeOffset bookingStart,
        DateTimeOffset bookingEnd,
        IReadOnlyCollection<WorkSchedule> schedules)
    {
        var businessStartTime =
            Common.Helpers.BusinessTime.GetBusinessTime(
                bookingStart);

        var businessEndTime =
            Common.Helpers.BusinessTime.GetBusinessTime(
                bookingEnd);

        var workDate =
            Common.Helpers.BusinessTime.GetBusinessDate(
                bookingStart);

        return schedules.Any(schedule =>
            schedule.WorkDate == workDate &&
            schedule.StartTime <= businessStartTime &&
            schedule.EndTime >= businessEndTime);
    }
}