using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Common.Helpers;
using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Bookings;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IServiceRepository serviceRepository,
    IStaffRepository staffRepository,
    IWorkScheduleRepository workScheduleRepository,
    TimeProvider timeProvider)
    : IBookingService
{
    public async Task<BookingResponse> CreateBookingAsync(
        long customerId,
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {

        var service = await serviceRepository.GetByIdAsync(
            request.ServiceId,
            cancellationToken);

        if (service is null)
        {
            throw new NotFoundException(
                "SERVICE_NOT_FOUND",
                "The selected service was not found.");
        }
        
        // A disabled service cannot be booked.
        if (!service.IsActive)
        {
            throw new BadRequestException(
                "SERVICE_INACTIVE",
                "The selected service is inactive.");
        }


        var staff = await staffRepository.GetByIdAsync(
            request.StaffId,
            cancellationToken);

        if (staff is null)
        {
            throw new NotFoundException(
                "STAFF_NOT_FOUND",
                "The selected staff was not found.");
        }
       
        // A disabled staff member cannot receive bookings.
        if (!staff.IsActive)
        {
            throw new BadRequestException(
                "STAFF_INACTIVE",
                "The selected staff is inactive.");
        }

        // Booking must not start in the past.
        if (request.StartTime < timeProvider.GetUtcNow())
        {
            throw new BadRequestException(
                "BOOKING_IN_PAST",
                "Booking start time cannot be in the past.");
        }

        // EndTime is calculated exclusively on the backend.
        var endTime = BookingTimeCalculator.CalculateEndTime(
            request.StartTime,
            service.DurationMinutes);

        var businessDate = BusinessTime.GetBusinessDate(request.StartTime);

        var schedules = await workScheduleRepository.GetByStaffAndDateAsync(
            request.StaffId,
            businessDate,
            cancellationToken);

        if (schedules.Count == 0)
        {
            throw new BadRequestException(
                "STAFF_NOT_WORKING",
                "The selected staff has no work schedule for the selected date.");
        }

        // The entire booking must fit inside one work schedule.
        var isWithinSchedule =
            BookingScheduleValidator.IsWithinSchedule(
                request.StartTime,
                endTime,
                schedules);

        if (!isWithinSchedule)
        {
            throw new BadRequestException(
                "BOOKING_OUTSIDE_WORKING_HOURS",
                "The booking must be completely within the staff working hours.");
        }

        // Conflict checking is performed against UTC DateTime values
        // because Booking.StartTime/EndTime are persisted as UTC.
        var newStartUtc = request.StartTime.UtcDateTime;
        var newEndUtc = endTime.UtcDateTime;

        var hasConflict =
            await bookingRepository.ExistsConflictAsync(
                request.StaffId,
                newStartUtc,
                newEndUtc,
                cancellationToken);

        if (hasConflict)
        {
            throw new ConflictException(
                "BOOKING_CONFLICT",
                "The selected time slot is already booked.");
        }

        var booking = new Models.Booking
        {
            BookingCode = BookingCodeGenerator.Generate(),

            CustomerId = customerId,
            ServiceId = request.ServiceId,
            StaffId = request.StaffId,

            StartTime = newStartUtc,
            EndTime = newEndUtc,

            Status = BookingStatus.Pending,

            CustomerNote = request.CustomerNote,
            CancellationReason = null,

            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        await bookingRepository.AddAsync(
            booking,
            cancellationToken);

        await bookingRepository.SaveChangesAsync(
            cancellationToken);

        return new BookingResponse
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,

            CustomerId = booking.CustomerId,
            ServiceId = booking.ServiceId,
            StaffId = booking.StaffId,

            ServiceName = service.Name,
            StaffName = staff.FullName,

            StartTime = booking.StartTime,
            EndTime = booking.EndTime,

            Status = booking.Status,

            CustomerNote = booking.CustomerNote,
            CancellationReason = booking.CancellationReason,

            CreatedAt = booking.CreatedAt
        };
    }

    public async Task<PagedResult<BookingResponse>> GetMyBookingsAsync(
        long customerId,
        BookingQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        if (parameters.Page < 1)
        {
            throw new BadRequestException(
                "INVALID_PAGE",
                "Page must be greater than or equal to 1.");
        }

        if (parameters.PageSize < 1 || parameters.PageSize > 100)
        {
            throw new BadRequestException(
                "INVALID_PAGE_SIZE",
                "PageSize must be between 1 and 100.");
        }

        var (items, totalCount) =
            await bookingRepository.GetMyBookingsAsync(
                customerId,
                parameters.Date,
                parameters.Status,
                parameters.Page,
                parameters.PageSize,
                cancellationToken);

        var responses = items
            .Select(booking => new BookingResponse
            {
                Id = booking.Id,
                BookingCode = booking.BookingCode,

                CustomerId = booking.CustomerId,
                ServiceId = booking.ServiceId,
                StaffId = booking.StaffId,

                ServiceName = booking.Service.Name,
                StaffName = booking.Staff.FullName,

                StartTime = booking.StartTime,
                EndTime = booking.EndTime,

                Status = booking.Status,

                CustomerNote = booking.CustomerNote,
                CancellationReason = booking.CancellationReason,

                CreatedAt = booking.CreatedAt
            })
            .ToList();

        return new PagedResult<BookingResponse>
        {
            Items = responses,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)parameters.PageSize)
        };
    }

    public async Task<BookingResponse> GetBookingAsync(
        long id,
        long? customerId,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdWithDetailsAsync(
            id,
            cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException(
                "BOOKING_NOT_FOUND",
                "The booking was not found.");
        }

        EnsureCustomerOwnsBooking(booking, customerId);
        return MapToResponse(booking);
    }

    public async Task<IReadOnlyList<AvailableSlotResponse>> GetAvailableSlotsAsync(
        AvailableSlotsQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        if (parameters.ServiceId <= 0 || parameters.StaffId <= 0 ||
            parameters.Date == default)
        {
            throw new BadRequestException(
                "INVALID_AVAILABILITY_QUERY",
                "ServiceId, StaffId, and Date are required.");
        }

        var service = await serviceRepository.GetByIdAsync(
            parameters.ServiceId,
            cancellationToken);

        if (service is null)
        {
            throw new NotFoundException(
                "SERVICE_NOT_FOUND",
                "The selected service was not found.");
        }

        if (!service.IsActive)
        {
            throw new BadRequestException(
                "SERVICE_INACTIVE",
                "The selected service is inactive.");
        }

        var staff = await staffRepository.GetByIdAsync(
            parameters.StaffId,
            cancellationToken);

        if (staff is null)
        {
            throw new NotFoundException(
                "STAFF_NOT_FOUND",
                "The selected staff was not found.");
        }

        if (!staff.IsActive)
        {
            throw new BadRequestException(
                "STAFF_INACTIVE",
                "The selected staff is inactive.");
        }

        var schedules = await workScheduleRepository.GetByStaffAndDateAsync(
            parameters.StaffId,
            parameters.Date,
            cancellationToken);

        if (schedules.Count == 0)
        {
            return Array.Empty<AvailableSlotResponse>();
        }

        var (dayStartUtc, dayEndUtc) =
            BusinessTime.GetUtcRangeForBusinessDate(parameters.Date);

        var occupiedBookings =
            await bookingRepository.GetNonCancelledForStaffInRangeAsync(
                parameters.StaffId,
                dayStartUtc,
                dayEndUtc,
                cancellationToken);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var availableSlots = new List<AvailableSlotResponse>();

        foreach (var schedule in schedules)
        {
            for (var localStart = schedule.StartTime;
                 localStart.AddMinutes(service.DurationMinutes) <= schedule.EndTime;
                 localStart = localStart.AddMinutes(1))
            {
                var startUtc = BusinessTime.ToUtc(
                    schedule.WorkDate,
                    localStart);

                if (startUtc < nowUtc)
                {
                    continue;
                }

                var endUtc = startUtc.AddMinutes(service.DurationMinutes);
                var hasConflict = occupiedBookings.Any(booking =>
                    startUtc < booking.EndTime &&
                    endUtc > booking.StartTime);

                if (!hasConflict)
                {
                    availableSlots.Add(new AvailableSlotResponse
                    {
                        StartTime = new DateTimeOffset(startUtc),
                        EndTime = new DateTimeOffset(endUtc)
                    });
                }
            }
        }

        return availableSlots;
    }

    public async Task<PagedResult<BookingResponse>> GetBookingsAsync(
        BookingQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        ValidateQueryParameters(parameters);

        var (items, totalCount) = await bookingRepository.GetPagedAsync(
            parameters.Date,
            parameters.Status,
            parameters.Page,
            parameters.PageSize,
            cancellationToken);

        return new PagedResult<BookingResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)parameters.PageSize)
        };
    }

    public async Task CancelBookingAsync(
        long id,
        long? customerId,
        string cancellationReason,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cancellationReason))
        {
            throw new BadRequestException(
                "CANCELLATION_REASON_REQUIRED",
                "CancellationReason is required.");
        }

        var booking = await bookingRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException(
                "BOOKING_NOT_FOUND",
                "The booking was not found.");
        }

        EnsureCustomerOwnsBooking(booking, customerId);

        if (booking.Status == BookingStatus.Completed ||
            booking.Status == BookingStatus.Cancelled)
        {
            throw new BadRequestException(
                "INVALID_BOOKING_STATUS",
                "This booking cannot be cancelled in its current status.");
        }

        if (booking.StartTime <= timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new BadRequestException(
                "BOOKING_ALREADY_STARTED",
                "A booking cannot be cancelled after it has started.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = cancellationReason.Trim();
        await bookingRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBookingStatusAsync(
        long id,
        BookingStatus status,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException(
                "BOOKING_NOT_FOUND",
                "The booking was not found.");
        }

        var isAllowedTransition =
            booking.Status == BookingStatus.Pending &&
            status == BookingStatus.Confirmed ||
            booking.Status == BookingStatus.Confirmed &&
            status == BookingStatus.Completed;

        if (!isAllowedTransition)
        {
            throw new BadRequestException(
                "INVALID_BOOKING_STATUS",
                $"Cannot change booking status from {booking.Status} to {status}.");
        }

        booking.Status = status;
        await bookingRepository.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateQueryParameters(
        BookingQueryParameters parameters)
    {
        if (parameters.Page < 1)
        {
            throw new BadRequestException(
                "INVALID_PAGE",
                "Page must be greater than or equal to 1.");
        }

        if (parameters.PageSize is < 1 or > 100)
        {
            throw new BadRequestException(
                "INVALID_PAGE_SIZE",
                "PageSize must be between 1 and 100.");
        }
    }

    private static void EnsureCustomerOwnsBooking(
        Models.Booking booking,
        long? customerId)
    {
        if (customerId.HasValue && booking.CustomerId != customerId.Value)
        {
            throw new ForbiddenException(
                "BOOKING_FORBIDDEN",
                "You are not allowed to access this booking.");
        }
    }

    private static BookingResponse MapToResponse(
        Models.Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            CustomerId = booking.CustomerId,
            ServiceId = booking.ServiceId,
            ServiceName = booking.Service.Name,
            StaffId = booking.StaffId,
            StaffName = booking.Staff.FullName,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status,
            CustomerNote = booking.CustomerNote,
            CancellationReason = booking.CancellationReason,
            CreatedAt = booking.CreatedAt
        };
    }
}