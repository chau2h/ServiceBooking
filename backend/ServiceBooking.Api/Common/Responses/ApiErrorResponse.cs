namespace ServiceBooking.Api.Common.Responses;

public sealed class ApiErrorResponse
{
    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string TraceId { get; init; } = string.Empty;
}