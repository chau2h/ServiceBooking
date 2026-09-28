using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Common.Responses;

namespace ServiceBooking.Api.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (
            context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or cancelled the request.
            // Do not convert normal request cancellation into HTTP 500.
            logger.LogDebug(
                "Request was cancelled by the client. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        catch (AppException ex)
        {
            await WriteErrorResponseAsync(
                context,
                ex.StatusCode,
                ex.Code,
                ex.Message);
        }
        catch (DbUpdateException ex)
        {
            // Database exceptions are intentionally not exposed to the client.
            // Detailed information is written to server logs instead.
            logger.LogError(
                ex,
                "Database update error. TraceId: {TraceId}",
                context.TraceIdentifier);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "DATABASE_ERROR",
                "A database error occurred.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception. TraceId: {TraceId}",
                context.TraceIdentifier);

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string code,
        string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            Code = code,
            Message = message,
            TraceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}