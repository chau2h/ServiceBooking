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
        // Business logic will be implemented in the next step.
        //
        // Planned flow:
        //
        // 1. Load Service
        // 2. Verify Service.IsActive
        // 3. Load Staff
        // 4. Verify Staff.IsActive
        // 5. Validate StartTime is not in the past
        // 6. Calculate EndTime using Service.DurationMinutes
        // 7. Verify booking fits WorkSchedule
        // 8. Check booking conflict
        // 9. Create Pending booking
        // 10. Persist booking

        throw new NotImplementedException();
    }
}