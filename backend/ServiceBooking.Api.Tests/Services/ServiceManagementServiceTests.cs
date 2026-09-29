using Moq;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Services;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services;
using Xunit;

namespace ServiceBooking.Api.Tests.Services;

public sealed class ServiceManagementServiceTests
{
    private readonly Mock<IServiceRepository> _repository = new();
    private readonly ServiceManagementService _sut;

    public ServiceManagementServiceTests()
    {
        _sut = new ServiceManagementService(_repository.Object);
        _repository.Setup(repository => repository.AddAsync(
                It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repository.Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Theory]
    [InlineData("", 30, 0, "SERVICE_NAME_REQUIRED")]
    [InlineData("   ", 30, 0, "SERVICE_NAME_REQUIRED")]
    [InlineData("Valid", 0, 0, "INVALID_SERVICE_DURATION")]
    [InlineData("Valid", -5, 0, "INVALID_SERVICE_DURATION")]
    [InlineData("Valid", 30, -0.01, "INVALID_SERVICE_PRICE")]
    public async Task CreateService_WhenInputViolatesBusinessValidation_ThrowsBadRequest(
        string name,
        int duration,
        decimal price,
        string code)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateServiceAsync(new CreateServiceRequest
            {
                Name = name,
                DurationMinutes = duration,
                Price = price
            }));

        Assert.Equal(code, exception.Code);
        _repository.Verify(repository => repository.AddAsync(
            It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateService_WhenValid_TrimsAndMapsResponse()
    {
        Service? added = null;
        _repository.Setup(repository => repository.AddAsync(
                It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Callback<Service, CancellationToken>((service, _) => added = service)
            .Returns(Task.CompletedTask);

        var response = await _sut.CreateServiceAsync(new CreateServiceRequest
        {
            Name = "  Haircut  ",
            Description = "  Style  ",
            DurationMinutes = 30,
            Price = 10m
        });

        Assert.NotNull(added);
        Assert.Equal("Haircut", added.Name);
        Assert.Equal("Style", added.Description);
        Assert.Equal(response.Name, added.Name);
        Assert.Equal(30, response.DurationMinutes);
        Assert.True(response.IsActive);
        _repository.Verify(repository => repository.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateService_ChangesActiveState(bool isActive)
    {
        var service = new Service
        {
            Id = 10,
            Name = "Haircut",
            DurationMinutes = 30,
            Price = 10m,
            IsActive = !isActive
        };
        _repository.Setup(repository => repository.GetByIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var response = await _sut.UpdateServiceAsync(10, new UpdateServiceRequest
        {
            Name = "Haircut",
            DurationMinutes = 30,
            Price = 10m,
            IsActive = isActive
        });

        Assert.Equal(isActive, response.IsActive);
        Assert.Equal(isActive, service.IsActive);
    }

    [Fact]
    public async Task GetServices_WhenPagingIsInvalid_ThrowsBadRequest()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.GetServicesAsync(new ServiceQueryParameters { Page = 1, PageSize = 101 }));

        Assert.Equal("INVALID_PAGE_SIZE", exception.Code);
        _repository.Verify(repository => repository.GetPagedAsync(
            It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetServices_ForwardsSearchAndFiltersAndMapsPagedMetadata()
    {
        _repository.Setup(repository => repository.GetPagedAsync(
                "cut", true, 2, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Service>
            {
                Items = [new Service
                {
                    Id = 10,
                    Name = "Haircut",
                    DurationMinutes = 30,
                    Price = 10m,
                    IsActive = true
                }],
                Page = 2,
                PageSize = 3,
                TotalItems = 7,
                TotalPages = 3
            });

        var result = await _sut.GetServicesAsync(new ServiceQueryParameters
        {
            Search = "cut",
            IsActive = true,
            Page = 2,
            PageSize = 3
        });

        var service = Assert.Single(result.Items);
        Assert.Equal("Haircut", service.Name);
        Assert.Equal(2, result.Page);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(7, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task UpdateService_WhenServiceDoesNotExist_ThrowsNotFound()
    {
        _repository.Setup(repository => repository.GetByIdAsync(
                55, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateServiceAsync(55, new UpdateServiceRequest
            {
                Name = "Haircut",
                DurationMinutes = 30,
                Price = 10m,
                IsActive = true
            }));

        Assert.Equal("SERVICE_NOT_FOUND", exception.Code);
    }
}
