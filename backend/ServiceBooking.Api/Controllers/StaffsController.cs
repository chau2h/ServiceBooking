using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.Common.Constants;
using ServiceBooking.Api.DTOs.Staffs;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Controllers;

[ApiController]
[Route("api/staffs")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class StaffsController(
    IStaffService staffService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<StaffResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<StaffResponse>>> GetStaffs(
        [FromQuery] StaffQueryParameters query,
        CancellationToken cancellationToken)
    {
        var response = await staffService.GetStaffsAsync(
            query,
            cancellationToken);

        return Ok(response);
    }
}