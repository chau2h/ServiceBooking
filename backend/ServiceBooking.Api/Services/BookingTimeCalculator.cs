using ServiceBooking.Api.Common.Exceptions;

namespace ServiceBooking.Api.Services;

public static class BookingTimeCalculator
{
    public static DateTimeOffset CalculateEndTime(
        DateTimeOffset startTime,
        int durationMinutes)
    {
        if (durationMinutes <= 0)
        {
            throw new BadRequestException(
                "INVALID_SERVICE_DURATION",
                "Service duration must be greater than zero.");
        }

        try
        {
            return startTime.AddMinutes(durationMinutes);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new BadRequestException(
                "INVALID_BOOKING_TIME",
                "The booking time is outside the supported date range.");
        }
    }
}