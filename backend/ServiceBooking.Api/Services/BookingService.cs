using ServiceBooking.Api.Common.Exceptions;
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
        // Rule 5.1 starts by loading the selected service because
        // DurationMinutes belongs to the Service entity.
        var service = await serviceRepository.GetByIdAsync(
            request.ServiceId,
            cancellationToken);

        if (service is null)
        {
            throw new NotFoundException(
                "SERVICE_NOT_FOUND",
                $"Service with id {request.ServiceId} was not found.");
        }

        // IMPORTANT:
        // EndTime is NOT accepted from the client.
        // It is always calculated on the backend from:
        //
        // EndTime = StartTime + Service.DurationMinutes
        var endTime = BookingTimeCalculator.CalculateEndTime(
            request.StartTime,
            service.DurationMinutes);

        // Rule 5.2 and 5.3 will be implemented in the next steps.
        // We intentionally stop here so that each business rule
        // remains independently testable and reviewable.
        _ = endTime;
        _ = customerId;
        _ = bookingRepository;
        _ = staffRepository;
        _ = workScheduleRepository;

        throw new NotImplementedException();
    }
}