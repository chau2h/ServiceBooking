using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Common.Helpers;
using ServiceBooking.Api.DTOs.Bookings;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public sealed class BookingService(
    IBookingRepository bookingRepository,
    IServiceRepository serviceRepository,
    IStaffRepository staffRepository,
    IWorkScheduleRepository workScheduleRepository)
    : IBookingService
{
    public async Task<BookingResponse> CreateBookingAsync(
        long customerId,
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Load Service.
        var service = await serviceRepository.GetByIdAsync(
            request.ServiceId,
            cancellationToken);

        if (service is null)
        {
            throw new NotFoundException(
                "SERVICE_NOT_FOUND",
                $"Service with id {request.ServiceId} was not found.");
        }

        // Rule 5.2:
        // A disabled service cannot be booked.
        if (!service.IsActive)
        {
            throw new BadRequestException(
                "SERVICE_INACTIVE",
                "The selected service is currently unavailable.");
        }

        // 2. Load Staff.
        var staff = await staffRepository.GetByIdAsync(
            request.StaffId,
            cancellationToken);

        if (staff is null)
        {
            throw new NotFoundException(
                "STAFF_NOT_FOUND",
                $"Staff with id {request.StaffId} was not found.");
        }

        // Rule 5.2:
        // A disabled staff member cannot receive bookings.
        if (!staff.IsActive)
        {
            throw new BadRequestException(
                "STAFF_INACTIVE",
                "The selected staff member is currently unavailable.");
        }

        // 3. Rule 5.2:
        // Booking must not start in the past.
        var now = BusinessTime.Now;

        if (request.StartTime < now)
        {
            throw new BadRequestException(
                "BOOKING_IN_PAST",
                "Booking start time cannot be in the past.");
        }

        // 4. Rule 5.1:
        // EndTime is calculated exclusively on the backend.
        var endTime = BookingTimeCalculator.CalculateEndTime(
            request.StartTime,
            service.DurationMinutes);

        // 5. Convert booking timestamps to Vietnam business time
        // to compare against WorkSchedule's Date/Time fields.
        var workDate =
            BusinessTime.GetBusinessDate(request.StartTime);

        var schedules =
            await workScheduleRepository.GetByStaffAndDateAsync(
                request.StaffId,
                workDate,
                cancellationToken);

        if (schedules.Count == 0)
        {
            throw new BadRequestException(
                "NO_WORK_SCHEDULE",
                "The selected staff member has no work schedule for this date.");
        }

        // 6. Rule 5.2:
        // The ENTIRE booking interval must fit inside a work schedule.
        var isWithinSchedule =
            BookingScheduleValidator.IsWithinSchedule(
                request.StartTime,
                endTime,
                schedules);

        if (!isWithinSchedule)
        {
            throw new BadRequestException(
                "OUTSIDE_WORK_SCHEDULE",
                "The booking must be completely within the staff's working hours.");
        }

        // Rule 5.3 will be added in the next step.
        throw new NotImplementedException();
    }
}