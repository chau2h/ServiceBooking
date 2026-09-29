using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceBooking.Api.Common.Constants;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Controllers;
using ServiceBooking.Api.DTOs.Bookings;
using ServiceBooking.Api.Services.Interfaces;
using Xunit;

namespace ServiceBooking.Api.Tests.Controllers;

public sealed class BookingControllerTests
{
    [Fact]
    public async Task CreateBooking_ExtractsCustomerIdFromNameIdentifierClaim()
    {
        var service = new Mock<IBookingService>();
        service.Setup(x => x.CreateBookingAsync(
                42, It.IsAny<CreateBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingResponse { Id = 9 });
        var controller = Controller(service.Object,
            new Claim(ClaimTypes.NameIdentifier, "42"));

        var result = await controller.CreateBooking(
            new CreateBookingRequest(), CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        service.Verify(x => x.CreateBookingAsync(
            42, It.IsAny<CreateBookingRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyBookings_ExtractsSubClaimWhenNameIdentifierIsMissing()
    {
        var service = new Mock<IBookingService>();
        service.Setup(x => x.GetMyBookingsAsync(
                52, It.IsAny<BookingQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Common.Responses.PagedResult<BookingResponse>());
        var controller = Controller(service.Object, new Claim("sub", "52"));

        var result = await controller.GetMyBookings(
            new BookingQueryParameters(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        service.Verify(x => x.GetMyBookingsAsync(
            52, It.IsAny<BookingQueryParameters>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_WhenClaimIsInvalid_ThrowsUnauthorizedWithContractCode()
    {
        var controller = Controller(new Mock<IBookingService>().Object,
            new Claim(ClaimTypes.NameIdentifier, "not-an-id"));

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            controller.CreateBooking(new CreateBookingRequest(), CancellationToken.None));

        Assert.Equal("INVALID_AUTHENTICATION_CONTEXT", exception.Code);
    }

    [Fact]
    public void UpdateBookingStatus_RequiresAdminAuthorizationMetadata()
    {
        var method = typeof(BookingController).GetMethod(nameof(BookingController.UpdateBookingStatus));
        var authorization = Assert.Single(method!.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.AdminOnly, authorization.Policy);
    }

    private static BookingController Controller(
        IBookingService service,
        params Claim[] claims)
    {
        var controller = new BookingController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "unit-test"))
                }
            }
        };

        return controller;
    }
}
