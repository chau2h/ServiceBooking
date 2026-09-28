namespace ServiceBooking.Api.Common.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(
        string code,
        string message)
        : base(code, message, StatusCodes.Status409Conflict)
    {
    }
}