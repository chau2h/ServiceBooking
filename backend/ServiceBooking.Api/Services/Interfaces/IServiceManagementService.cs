using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Services;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IServiceManagementService
{
    Task<PagedResult<ServiceResponse>> GetServicesAsync(
        ServiceQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> CreateServiceAsync(
        CreateServiceRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> UpdateServiceAsync(
        long id,
        UpdateServiceRequest request,
        CancellationToken cancellationToken = default);
}