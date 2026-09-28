using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class ServiceManagementService(
    IServiceRepository serviceRepository)
    : IServiceManagementService
{
}