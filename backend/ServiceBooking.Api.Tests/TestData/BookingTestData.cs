using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Tests.TestData;

internal static class BookingTestData
{
    public static readonly DateTimeOffset Now =
        new(2035, 1, 3, 1, 0, 0, TimeSpan.Zero);

    public static readonly DateTimeOffset FutureStart =
        new(2035, 1, 3, 4, 0, 0, TimeSpan.Zero);

    public static Service Service(
        bool isActive = true,
        int durationMinutes = 60) => new()
    {
        Id = 10,
        Name = "Haircut",
        DurationMinutes = durationMinutes,
        Price = 100m,
        IsActive = isActive
    };

    public static Staff Staff(bool isActive = true) => new()
    {
        Id = 20,
        FullName = "Taylor Staff",
        Email = "staff@example.test",
        IsActive = isActive
    };

    public static Booking Booking(
        long customerId = 1,
        BookingStatus status = BookingStatus.Pending,
        DateTime? startTime = null) => new()
    {
        Id = 30,
        BookingCode = "BK-TEST-0001",
        CustomerId = customerId,
        ServiceId = 10,
        StaffId = 20,
        StartTime = startTime ?? FutureStart.UtcDateTime,
        EndTime = (startTime ?? FutureStart.UtcDateTime).AddHours(1),
        Status = status,
        CustomerNote = "Customer note",
        CancellationReason = status == BookingStatus.Cancelled
            ? "Previously cancelled"
            : null,
        CreatedAt = Now.UtcDateTime,
        Service = Service(),
        Staff = Staff()
    };

    public static WorkSchedule Schedule(
        TimeOnly start = default,
        TimeOnly end = default) => new()
    {
        StaffId = 20,
        WorkDate = DateOnly.FromDateTime(
            FutureStart.ToOffset(TimeSpan.FromHours(7)).DateTime),
        StartTime = start == default ? new TimeOnly(9, 0) : start,
        EndTime = end == default ? new TimeOnly(17, 0) : end
    };
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
