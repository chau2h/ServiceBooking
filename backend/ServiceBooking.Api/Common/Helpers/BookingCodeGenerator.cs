namespace ServiceBooking.Api.Common.Helpers;

public static class BookingCodeGenerator
{
    public static string Generate()
    {
        // Guid giúp BookingCode có xác suất trùng cực thấp.
        var uniquePart = Guid.NewGuid()
            .ToString("N")
            .ToUpperInvariant()[..10];

        return $"BK-{DateTime.UtcNow:yyyyMMdd}-{uniquePart}";
    }
}