using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.Common.Constants;
using ServiceBooking.Api.DTOs.Schedules;
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

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(
        typeof(StaffResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffResponse>> CreateStaff(
        [FromBody] CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var response = await staffService.CreateStaffAsync(
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:long}")]
    [ProducesResponseType(
        typeof(StaffResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffResponse>> UpdateStaff(
        long id,
        [FromBody] UpdateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var response = await staffService.UpdateStaffAsync(
            id,
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:long}/schedules")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ScheduleResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ScheduleResponse>>>
        GetSchedules(
            long id,
            CancellationToken cancellationToken)
    {
        var response = await staffService.GetSchedulesAsync(
            id,
            cancellationToken);

        return Ok(response);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("{id:long}/schedules")]
    [ProducesResponseType(
        typeof(ScheduleResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScheduleResponse>>
        CreateSchedule(
            long id,
            [FromBody] CreateScheduleRequest request,
            CancellationToken cancellationToken)
    {
        var response =
            await staffService.CreateScheduleAsync(
                id,
                request,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }
}