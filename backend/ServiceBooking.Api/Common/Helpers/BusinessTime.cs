namespace ServiceBooking.Api.Common.Helpers;

public static class BusinessTime
{
    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "SE Asia Standard Time"
                : "Asia/Ho_Chi_Minh");

    public static DateTimeOffset Now =>
        TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow,
            VietnamTimeZone);

    public static DateTime ToUtc(
        DateTimeOffset value)
    {
        return value.UtcDateTime;
    }

    public static DateTime ToUtc(
        DateOnly date,
        TimeOnly time)
    {
        var localDateTime = DateTime.SpecifyKind(
            date.ToDateTime(time),
            DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(
            localDateTime,
            VietnamTimeZone);
    }

    public static DateOnly GetBusinessDate(
        DateTimeOffset value)
    {
        var local = TimeZoneInfo.ConvertTime(
            value,
            VietnamTimeZone);

        return DateOnly.FromDateTime(
            local.DateTime);
    }

    public static TimeOnly GetBusinessTime(
        DateTimeOffset value)
    {
        var local = TimeZoneInfo.ConvertTime(
            value,
            VietnamTimeZone);

        return TimeOnly.FromDateTime(
            local.DateTime);
    }

    public static (DateTime StartUtc, DateTime EndUtc) GetUtcRangeForBusinessDate(
        DateOnly date)
    {
        var localStart = date.ToDateTime(TimeOnly.MinValue);
        var localEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(
            localStart,
            VietnamTimeZone);

        var endUtc = TimeZoneInfo.ConvertTimeToUtc(
            localEnd,
            VietnamTimeZone);

        return (startUtc, endUtc);
    }
}