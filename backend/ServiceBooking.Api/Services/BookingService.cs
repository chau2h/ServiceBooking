using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IServiceRepository serviceRepository,
    IStaffRepository staffRepository,
    IWorkScheduleRepository workScheduleRepository)
    : IBookingService
{
}