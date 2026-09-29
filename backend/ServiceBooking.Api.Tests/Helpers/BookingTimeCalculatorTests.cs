using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Services;
using Xunit;

namespace ServiceBooking.Api.Tests.Helpers;

public sealed class BookingTimeCalculatorTests
{
    [Theory]
    [InlineData("2035-01-03T09:15:00+07:00", 45, "2035-01-03T10:00:00+07:00")]
    [InlineData("2035-01-03T09:00:00+07:00", 30, "2035-01-03T09:30:00+07:00")]
    [InlineData("2035-01-03T09:00:00+07:00", 60, "2035-01-03T10:00:00+07:00")]
    [InlineData("2035-01-03T09:00:00+07:00", 600, "2035-01-03T19:00:00+07:00")]
    [InlineData("2035-01-03T23:45:00+07:00", 30, "2035-01-04T00:15:00+07:00")]
    public void CalculateEndTime_AddsDurationToStart(
        string start,
        int durationMinutes,
        string expectedEnd)
    {
        var result = BookingTimeCalculator.CalculateEndTime(
            DateTimeOffset.Parse(start),
            durationMinutes);

        Assert.Equal(DateTimeOffset.Parse(expectedEnd), result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateEndTime_WhenDurationIsNotPositive_ThrowsBadRequest(
        int durationMinutes)
    {
        var exception = Assert.Throws<BadRequestException>(() =>
            BookingTimeCalculator.CalculateEndTime(
                DateTimeOffset.Parse("2035-01-03T09:00:00+07:00"),
                durationMinutes));

        Assert.Equal("INVALID_SERVICE_DURATION", exception.Code);
    }

    [Fact]
    public void CalculateEndTime_WhenDateOverflows_ThrowsInvalidBookingTime()
    {
        var exception = Assert.Throws<BadRequestException>(() =>
            BookingTimeCalculator.CalculateEndTime(
                DateTimeOffset.MaxValue,
                1));

        Assert.Equal("INVALID_BOOKING_TIME", exception.Code);
    }
}
