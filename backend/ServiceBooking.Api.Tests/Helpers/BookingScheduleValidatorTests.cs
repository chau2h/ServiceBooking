using ServiceBooking.Api.Models;
using ServiceBooking.Api.Services;
using Xunit;

namespace ServiceBooking.Api.Tests.Helpers;

public sealed class BookingScheduleValidatorTests
{
    private static readonly DateOnly WorkDate = new(2035, 1, 3);

    [Theory]
    [InlineData("09:00", "10:00", "09:00", "17:00", true)]
    [InlineData("10:00", "11:00", "09:00", "17:00", true)]
    [InlineData("16:00", "17:00", "09:00", "17:00", true)]
    [InlineData("09:00", "17:00", "09:00", "17:00", true)]
    [InlineData("08:30", "10:00", "09:00", "17:00", false)]
    [InlineData("16:30", "17:30", "09:00", "17:00", false)]
    public void IsWithinSchedule_RequiresWholeIntervalInsideOneSchedule(
        string start,
        string end,
        string scheduleStart,
        string scheduleEnd,
        bool expected)
    {
        var schedules = new[]
        {
            Schedule(scheduleStart, scheduleEnd)
        };

        var result = BookingScheduleValidator.IsWithinSchedule(
            AtBusinessTime(start),
            AtBusinessTime(end),
            schedules);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsWithinSchedule_DoesNotCombineSeparateSchedules()
    {
        var schedules = new[]
        {
            Schedule("09:00", "12:00"),
            Schedule("13:00", "17:00")
        };

        var result = BookingScheduleValidator.IsWithinSchedule(
            AtBusinessTime("11:30"),
            AtBusinessTime("13:30"),
            schedules);

        Assert.False(result);
    }

    [Fact]
    public void IsWithinSchedule_WhenNoScheduleExists_ReturnsFalse()
    {
        var result = BookingScheduleValidator.IsWithinSchedule(
            AtBusinessTime("10:00"),
            AtBusinessTime("11:00"),
            Array.Empty<WorkSchedule>());

        Assert.False(result);
    }

    [Fact]
    public void IsWithinSchedule_WhenBookingFitsOneOfMultipleSchedules_ReturnsTrue()
    {
        var schedules = new[]
        {
            Schedule("09:00", "12:00"),
            Schedule("13:00", "17:00")
        };

        var result = BookingScheduleValidator.IsWithinSchedule(
            AtBusinessTime("14:00"),
            AtBusinessTime("15:00"),
            schedules);

        Assert.True(result);
    }

    private static WorkSchedule Schedule(string start, string end) => new()
    {
        WorkDate = WorkDate,
        StartTime = TimeOnly.Parse(start),
        EndTime = TimeOnly.Parse(end)
    };

    private static DateTimeOffset AtBusinessTime(string time) =>
        new(WorkDate.ToDateTime(TimeOnly.Parse(time)), TimeSpan.FromHours(7));
}
