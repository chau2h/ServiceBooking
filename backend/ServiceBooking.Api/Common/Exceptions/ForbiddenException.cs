namespace ServiceBooking.Api.Common.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(
        string code,
        string message)
        : base(code, message, StatusCodes.Status403Forbidden)
    {
    }
}