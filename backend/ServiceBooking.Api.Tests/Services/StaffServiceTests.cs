using Moq;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.DTOs.Schedules;
using ServiceBooking.Api.DTOs.Staffs;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services;
using Xunit;

namespace ServiceBooking.Api.Tests.Services;

public sealed class StaffServiceTests
{
    private readonly Mock<IStaffRepository> _staff = new();
    private readonly Mock<IWorkScheduleRepository> _schedules = new();
    private readonly StaffService _sut;

    public StaffServiceTests()
    {
        _sut = new StaffService(_staff.Object, _schedules.Object);
        _staff.Setup(repository => repository.GetByEmailAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);
        _staff.Setup(repository => repository.AddAsync(
                It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _staff.Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _staff.Setup(repository => repository.ExistsAsync(
                20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _schedules.Setup(repository => repository.AddAsync(
                It.IsAny<WorkSchedule>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _schedules.Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Theory]
    [InlineData("", "staff@example.test", "STAFF_NAME_REQUIRED")]
    [InlineData("   ", "staff@example.test", "STAFF_NAME_REQUIRED")]
    [InlineData("Staff", " ", "STAFF_EMAIL_REQUIRED")]
    public async Task CreateStaff_WhenRequiredTextIsMissing_ThrowsBadRequest(
        string fullName,
        string email,
        string code)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateStaffAsync(new CreateStaffRequest
            {
                FullName = fullName,
                Email = email
            }));

        Assert.Equal(code, exception.Code);
    }

    [Fact]
    public async Task CreateStaff_WhenEmailAlreadyExists_ThrowsConflict()
    {
        _staff.Setup(repository => repository.GetByEmailAsync(
                "staff@example.test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Staff { Id = 22, Email = "staff@example.test" });

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateStaffAsync(new CreateStaffRequest
            {
                FullName = "Staff",
                Email = " STAFF@example.test "
            }));

        Assert.Equal("STAFF_EMAIL_EXISTS", exception.Code);
    }

    [Fact]
    public async Task CreateStaff_WhenValid_NormalizesEmailAndReturnsSafeStaffResponse()
    {
        Staff? created = null;
        _staff.Setup(repository => repository.AddAsync(
                It.IsAny<Staff>(), It.IsAny<CancellationToken>()))
            .Callback<Staff, CancellationToken>((staff, _) => created = staff)
            .Returns(Task.CompletedTask);

        var response = await _sut.CreateStaffAsync(new CreateStaffRequest
        {
            FullName = " Taylor Staff ",
            Email = " TAYLOR@example.test "
        });

        Assert.NotNull(created);
        Assert.Equal("Taylor Staff", response.FullName);
        Assert.Equal("taylor@example.test", created.Email);
        Assert.True(response.IsActive);
    }

    [Fact]
    public async Task CreateSchedule_WhenStartIsNotBeforeEnd_ThrowsBadRequest()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateScheduleAsync(20, new CreateScheduleRequest
            {
                WorkDate = new DateOnly(2035, 1, 3),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(10, 0)
            }));

        Assert.Equal("INVALID_SCHEDULE_TIME", exception.Code);
        _schedules.Verify(repository => repository.ExistsOverlapAsync(
            It.IsAny<long>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(),
            It.IsAny<TimeOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSchedule_WhenOverlapExists_ThrowsConflict()
    {
        _schedules.Setup(repository => repository.ExistsOverlapAsync(
                20, It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateScheduleAsync(20, new CreateScheduleRequest
            {
                WorkDate = new DateOnly(2035, 1, 3),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0)
            }));

        Assert.Equal("SCHEDULE_CONFLICT", exception.Code);
        _schedules.Verify(repository => repository.AddAsync(
            It.IsAny<WorkSchedule>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSchedule_WhenAdjacentIntervalHasNoConflict_CreatesSchedule()
    {
        WorkSchedule? created = null;
        _schedules.Setup(repository => repository.ExistsOverlapAsync(
                20, It.IsAny<DateOnly>(), new TimeOnly(10, 0), new TimeOnly(11, 0),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _schedules.Setup(repository => repository.AddAsync(
                It.IsAny<WorkSchedule>(), It.IsAny<CancellationToken>()))
            .Callback<WorkSchedule, CancellationToken>((schedule, _) => created = schedule)
            .Returns(Task.CompletedTask);

        var response = await _sut.CreateScheduleAsync(20, new CreateScheduleRequest
        {
            WorkDate = new DateOnly(2035, 1, 3),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0)
        });

        Assert.NotNull(created);
        Assert.Equal(20, response.StaffId);
        Assert.Equal(new TimeOnly(10, 0), response.StartTime);
    }

    [Fact]
    public async Task GetSchedules_WhenStaffExists_MapsScheduleResponse()
    {
        _schedules.Setup(repository => repository.GetByStaffAsync(
                20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new WorkSchedule
            {
                Id = 7,
                StaffId = 20,
                WorkDate = new DateOnly(2035, 1, 3),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0)
            }]);

        var result = await _sut.GetSchedulesAsync(20);

        var schedule = Assert.Single(result);
        Assert.Equal(7, schedule.Id);
        Assert.Equal(20, schedule.StaffId);
        Assert.Equal(new TimeOnly(9, 0), schedule.StartTime);
        Assert.Equal(new TimeOnly(17, 0), schedule.EndTime);
    }

    [Fact]
    public async Task GetSchedules_WhenStaffDoesNotExist_ThrowsNotFound()
    {
        _staff.Setup(repository => repository.ExistsAsync(
                20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetSchedulesAsync(20));

        Assert.Equal("STAFF_NOT_FOUND", exception.Code);
        _schedules.Verify(repository => repository.GetByStaffAsync(
            It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
