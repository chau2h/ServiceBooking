using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.DTOs.Schedules;
using ServiceBooking.Api.DTOs.Staffs;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public sealed class StaffService(
    IStaffRepository staffRepository,
    IWorkScheduleRepository workScheduleRepository)
    : IStaffService
{
    public async Task<IReadOnlyList<StaffResponse>> GetStaffsAsync(
        StaffQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var staffs = await staffRepository.GetAllAsync(
            query.IsActive,
            cancellationToken);

        return staffs
            .Select(MapToStaffResponse)
            .ToList();
    }

    public async Task<StaffResponse> CreateStaffAsync(
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var fullName = NormalizeRequiredText(
            request.FullName,
            "STAFF_NAME_REQUIRED",
            "Staff full name is required.");

        var email = NormalizeEmail(request.Email);

        var existingStaff =
            await staffRepository.GetByEmailAsync(
                email,
                cancellationToken);

        if (existingStaff is not null)
        {
            throw new ConflictException(
                "STAFF_EMAIL_EXISTS",
                "A staff member with this email already exists.");
        }

        var staff = new Staff
        {
            FullName = fullName,
            Email = email,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await staffRepository.AddAsync(
            staff,
            cancellationToken);

        await staffRepository.SaveChangesAsync(
            cancellationToken);

        return MapToStaffResponse(staff);
    }

    public async Task<StaffResponse> UpdateStaffAsync(
        long id,
        UpdateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var fullName = NormalizeRequiredText(
            request.FullName,
            "STAFF_NAME_REQUIRED",
            "Staff full name is required.");

        var email = NormalizeEmail(request.Email);

        var staff = await staffRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (staff is null)
        {
            throw new NotFoundException(
                "STAFF_NOT_FOUND",
                $"Staff with id {id} was not found.");
        }

        var existingStaff =
            await staffRepository.GetByEmailAsync(
                email,
                cancellationToken);

        // An unchanged email belongs to this same Staff and is valid.
        // A different Staff using the same email is a conflict.
        if (existingStaff is not null &&
            existingStaff.Id != id)
        {
            throw new ConflictException(
                "STAFF_EMAIL_EXISTS",
                "A staff member with this email already exists.");
        }

        staff.FullName = fullName;
        staff.Email = email;
        staff.IsActive = request.IsActive;
        staff.UpdatedAt = DateTime.UtcNow;

        await staffRepository.SaveChangesAsync(
            cancellationToken);

        return MapToStaffResponse(staff);
    }

    public async Task<IReadOnlyList<ScheduleResponse>> GetSchedulesAsync(
        long staffId,
        CancellationToken cancellationToken = default)
    {
        await EnsureStaffExistsAsync(
            staffId,
            cancellationToken);

        var schedules =
            await workScheduleRepository.GetByStaffAsync(
                staffId,
                cancellationToken);

        return schedules
            .Select(MapToScheduleResponse)
            .ToList();
    }

    public async Task<ScheduleResponse> CreateScheduleAsync(
        long staffId,
        CreateScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureStaffExistsAsync(
            staffId,
            cancellationToken);

        ValidateSchedule(request);

        var hasOverlap =
            await workScheduleRepository.ExistsOverlapAsync(
                staffId,
                request.WorkDate,
                request.StartTime,
                request.EndTime,
                cancellationToken);

        if (hasOverlap)
        {
            throw new ConflictException(
                "SCHEDULE_CONFLICT",
                "The new schedule overlaps with an existing schedule.");
        }

        var schedule = new WorkSchedule
        {
            StaffId = staffId,
            WorkDate = request.WorkDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.UtcNow
        };

        await workScheduleRepository.AddAsync(
            schedule,
            cancellationToken);

        await workScheduleRepository.SaveChangesAsync(
            cancellationToken);

        return MapToScheduleResponse(schedule);
    }

    private async Task EnsureStaffExistsAsync(
        long staffId,
        CancellationToken cancellationToken)
    {
        var exists = await staffRepository.ExistsAsync(
            staffId,
            cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(
                "STAFF_NOT_FOUND",
                $"Staff with id {staffId} was not found.");
        }
    }

    private static void ValidateSchedule(
        CreateScheduleRequest request)
    {
        if (request.StartTime >= request.EndTime)
        {
            throw new BadRequestException(
                "INVALID_SCHEDULE_TIME",
                "StartTime must be earlier than EndTime.");
        }
    }

    private static string NormalizeRequiredText(
        string value,
        string errorCode,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException(
                errorCode,
                errorMessage);
        }

        return value.Trim();
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException(
                "STAFF_EMAIL_REQUIRED",
                "Staff email is required.");
        }

        return email.Trim().ToLowerInvariant();
    }

    private static StaffResponse MapToStaffResponse(
        Staff staff)
    {
        return new StaffResponse
        {
            Id = staff.Id,
            FullName = staff.FullName,
            Email = staff.Email,
            IsActive = staff.IsActive
        };
    }

    private static ScheduleResponse MapToScheduleResponse(
        WorkSchedule schedule)
    {
        return new ScheduleResponse
        {
            Id = schedule.Id,
            StaffId = schedule.StaffId,
            WorkDate = schedule.WorkDate,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime
        };
    }
}