using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Services;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public sealed class ServiceManagementService(
    IServiceRepository serviceRepository)
    : IServiceManagementService
{
    public async Task<PagedResult<ServiceResponse>> GetServicesAsync(
        ServiceQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        var result = await serviceRepository.GetPagedAsync(
            query.Search,
            query.IsActive,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = result.Items
            .Select(MapToResponse)
            .ToList();

        return new PagedResult<ServiceResponse>
        {
            Items = items,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }

    private static ServiceResponse MapToResponse(
        Service service)
    {
        return new ServiceResponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            IsActive = service.IsActive
        };
    }
}