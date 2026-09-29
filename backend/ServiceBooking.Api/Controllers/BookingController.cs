using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.Common.Constants;
using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Bookings;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Roles = Roles.Customer)]
public class BookingController(IBookingService bookingService)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var customerIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!long.TryParse(customerIdClaim, out var customerId))
        {
            return Unauthorized();
        }

        var result = await bookingService.CreateBookingAsync(
            customerId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(CreateBooking),
            new { id = result.Id },
            result);
    }

    [HttpGet("my-bookings")]
    [ProducesResponseType(
        typeof(PagedResult<BookingResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetMyBookings(
        [FromQuery] BookingQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var customerIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!long.TryParse(customerIdClaim, out var customerId))
        {
            return Unauthorized();
        }

        var result = await bookingService.GetMyBookingsAsync(
            customerId,
            parameters,
            cancellationToken);

        return Ok(result);
    }
}
