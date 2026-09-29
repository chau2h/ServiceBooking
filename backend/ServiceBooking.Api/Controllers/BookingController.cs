using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.Common.Constants;
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

        if (!int.TryParse(customerIdClaim, out var customerId))
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
}
