using ServiceBooking.Api.Common.Exceptions;
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

    public async Task<ServiceResponse> CreateServiceAsync(
        CreateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateServiceRequest(
            request.Name,
            request.DurationMinutes,
            request.Price);

        var service = new Service
        {
            Name = request.Name.Trim(),
            Description = NormalizeDescription(
                request.Description),
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await serviceRepository.AddAsync(
            service,
            cancellationToken);

        await serviceRepository.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(service);
    }

    public async Task<ServiceResponse> UpdateServiceAsync(
        long id,
        UpdateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateServiceRequest(
            request.Name,
            request.DurationMinutes,
            request.Price);

        var service = await serviceRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (service is null)
        {
            throw new NotFoundException(
                "SERVICE_NOT_FOUND",
                $"Service with id {id} was not found.");
        }

        service.Name = request.Name.Trim();
        service.Description = NormalizeDescription(
            request.Description);
        service.DurationMinutes = request.DurationMinutes;
        service.Price = request.Price;
        service.IsActive = request.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        await serviceRepository.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(service);
    }

    private static void ValidateServiceRequest(
        string name,
        int durationMinutes,
        decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException(
                "SERVICE_NAME_REQUIRED",
                "Service name is required.");
        }

        if (durationMinutes <= 0)
        {
            throw new BadRequestException(
                "INVALID_SERVICE_DURATION",
                "DurationMinutes must be greater than zero.");
        }

        if (price < 0)
        {
            throw new BadRequestException(
                "INVALID_SERVICE_PRICE",
                "Price cannot be negative.");
        }
    }

    private static string? NormalizeDescription(
        string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
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