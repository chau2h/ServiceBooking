using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.Common.Constants;
using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Services;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Controllers;

[ApiController]
[Route("api/services")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class ServicesController(
    IServiceManagementService serviceManagementService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<ServiceResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ServiceResponse>>> GetServices(
        [FromQuery] ServiceQueryParameters query,
        CancellationToken cancellationToken)
    {
        var response =
            await serviceManagementService.GetServicesAsync(
                query,
                cancellationToken);

        return Ok(response);
    }
}