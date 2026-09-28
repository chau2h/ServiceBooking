namespace ServiceBooking.Api.Common.Exceptions;

public sealed class BadRequestException : AppException
{
    public BadRequestException(
        string code,
        string message)
        : base(code, message, StatusCodes.Status400BadRequest)
    {
    }
}