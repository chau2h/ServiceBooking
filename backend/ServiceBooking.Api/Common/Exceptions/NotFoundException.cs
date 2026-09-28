namespace ServiceBooking.Api.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(
        string code,
        string message)
        : base(code, message, StatusCodes.Status404NotFound)
    {
    }
}