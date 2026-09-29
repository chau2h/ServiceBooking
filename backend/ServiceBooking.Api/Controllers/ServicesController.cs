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

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(
        typeof(ServiceResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ServiceResponse>> CreateService(
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await serviceManagementService.CreateServiceAsync(
                request,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:long}")]
    [ProducesResponseType(
        typeof(ServiceResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse>> UpdateService(
        long id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await serviceManagementService.UpdateServiceAsync(
                id,
                request,
                cancellationToken);

        return Ok(response);
    }
}