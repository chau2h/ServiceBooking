using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.Common.Constants;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Bookings;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public class BookingController(IBookingService bookingService)
    : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        var result = await bookingService.CreateBookingAsync(
            customerId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetBooking),
            new { id = result.Id },
            result);
    }

    [HttpGet("my-bookings")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(
        typeof(PagedResult<BookingResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetMyBookings(
        [FromQuery] BookingQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        var result = await bookingService.GetMyBookingsAsync(
            customerId,
            parameters,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("available-slots")]
    [ProducesResponseType(
        typeof(IReadOnlyList<AvailableSlotResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AvailableSlotResponse>>>
        GetAvailableSlots(
            [FromQuery] AvailableSlotsQueryParameters parameters,
            CancellationToken cancellationToken)
    {
        var result = await bookingService.GetAvailableSlotsAsync(
            parameters,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(
        typeof(PagedResult<BookingResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetBookings(
        [FromQuery] BookingQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await bookingService.GetBookingsAsync(
            parameters,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = Roles.Customer + "," + Roles.Admin)]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetBooking(
        long id,
        CancellationToken cancellationToken)
    {
        long? customerId = User.IsInRole(Roles.Admin)
            ? null
            : GetCustomerId();

        var result = await bookingService.GetBookingAsync(
            id,
            customerId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:long}/cancel")]
    [Authorize(Roles = Roles.Customer + "," + Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBooking(
        long id,
        [FromBody] CancelBookingRequest request,
        CancellationToken cancellationToken)
    {
        long? customerId = User.IsInRole(Roles.Admin)
            ? null
            : GetCustomerId();

        await bookingService.CancelBookingAsync(
            id,
            User.IsInRole(Roles.Admin) ? null : customerId,
            request.CancellationReason,
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:long}/status")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBookingStatus(
        long id,
        [FromBody] UpdateBookingStatusRequest request,
        CancellationToken cancellationToken)
    {
        await bookingService.UpdateBookingStatusAsync(
            id,
            request.Status,
            cancellationToken);

        return NoContent();
    }

    private long GetCustomerId()
    {
        var customerIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!long.TryParse(customerIdClaim, out var customerId))
        {
            throw new UnauthorizedException(
                "INVALID_AUTHENTICATION_CONTEXT",
                "The authenticated user could not be identified.");
        }

        return customerId;
    }
}
