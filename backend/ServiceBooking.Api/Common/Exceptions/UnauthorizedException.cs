namespace ServiceBooking.Api.Common.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(
        string code,
        string message)
        : base(code, message, StatusCodes.Status401Unauthorized)
    {
    }
}