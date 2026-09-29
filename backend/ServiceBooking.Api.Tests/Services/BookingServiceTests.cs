using Moq;
using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Common.Responses;
using ServiceBooking.Api.DTOs.Bookings;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services;
using ServiceBooking.Api.Tests.TestData;
using Xunit;

namespace ServiceBooking.Api.Tests.Services;

public sealed class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly Mock<IServiceRepository> _services = new();
    private readonly Mock<IStaffRepository> _staff = new();
    private readonly Mock<IWorkScheduleRepository> _schedules = new();
    private readonly FixedTimeProvider _clock = new(BookingTestData.Now);
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _sut = new BookingService(
            _bookings.Object,
            _services.Object,
            _staff.Object,
            _schedules.Object,
            _clock);

        _services.Setup(repository => repository.GetByIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Service());
        _staff.Setup(repository => repository.GetByIdAsync(
                20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Staff());
        _schedules.Setup(repository => repository.GetByStaffAndDateAsync(
                20, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([BookingTestData.Schedule()]);
        _bookings.Setup(repository => repository.ExistsConflictAsync(
                20, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _bookings.Setup(repository => repository.AddAsync(
                It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _bookings.Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CreateBooking_WhenValid_CreatesPendingBookingWithServerOwnedValues()
    {
        Booking? addedBooking = null;
        _bookings.Setup(repository => repository.AddAsync(
                It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .Callback<Booking, CancellationToken>((booking, _) => addedBooking = booking)
            .Returns(Task.CompletedTask);

        var request = ValidRequest(BookingTestData.FutureStart);
        var result = await _sut.CreateBookingAsync(99, request, CancellationToken.None);

        Assert.NotNull(addedBooking);
        Assert.Equal(99, addedBooking.CustomerId);
        Assert.Equal(10, addedBooking.ServiceId);
        Assert.Equal(20, addedBooking.StaffId);
        Assert.Equal(BookingTestData.FutureStart.UtcDateTime, addedBooking.StartTime);
        Assert.Equal(BookingTestData.FutureStart.AddMinutes(60).UtcDateTime, addedBooking.EndTime);
        Assert.Equal(BookingStatus.Pending, addedBooking.Status);
        Assert.StartsWith("BK-", addedBooking.BookingCode);
        Assert.Null(addedBooking.CancellationReason);
        Assert.Equal("Please call on arrival", addedBooking.CustomerNote);
        Assert.Equal(_clock.GetUtcNow().UtcDateTime, addedBooking.CreatedAt);
        Assert.Equal(addedBooking.EndTime, result.EndTime);
        _bookings.Verify(repository => repository.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBooking_WhenStartTimeIsInThePast_ThrowsBadRequest()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateBookingAsync(99, ValidRequest(BookingTestData.Now.AddTicks(-1)), CancellationToken.None));

        Assert.Equal("BOOKING_IN_PAST", exception.Code);
        _schedules.Verify(repository => repository.GetByStaffAndDateAsync(
            It.IsAny<long>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBooking_WhenStartEqualsNowBeforeSchedule_IsNotRejectedAsPast()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateBookingAsync(
                99,
                ValidRequest(BookingTestData.Now),
                CancellationToken.None));

        Assert.Equal("BOOKING_OUTSIDE_WORKING_HOURS", exception.Code);
    }

    [Fact]
    public async Task CreateBooking_WhenServiceDoesNotExist_ThrowsNotFound()
    {
        _services.Setup(repository => repository.GetByIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateBookingAsync(99, ValidRequest(), CancellationToken.None));

        Assert.Equal("SERVICE_NOT_FOUND", exception.Code);
    }

    [Fact]
    public async Task CreateBooking_WhenServiceIsInactive_ThrowsBadRequest()
    {
        _services.Setup(repository => repository.GetByIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Service(isActive: false));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateBookingAsync(99, ValidRequest(), CancellationToken.None));

        Assert.Equal("SERVICE_INACTIVE", exception.Code);
    }

    [Fact]
    public async Task CreateBooking_WhenStaffDoesNotExist_ThrowsNotFound()
    {
        _staff.Setup(repository => repository.GetByIdAsync(
                20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Staff?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateBookingAsync(99, ValidRequest(), CancellationToken.None));

        Assert.Equal("STAFF_NOT_FOUND", exception.Code);
    }

    [Fact]
    public async Task CreateBooking_WhenStaffIsInactive_ThrowsBadRequest()
    {
        _staff.Setup(repository => repository.GetByIdAsync(
                20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Staff(isActive: false));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateBookingAsync(99, ValidRequest(), CancellationToken.None));

        Assert.Equal("STAFF_INACTIVE", exception.Code);
    }

    [Fact]
    public async Task CreateBooking_WhenNoScheduleExists_ThrowsBadRequest()
    {
        _schedules.Setup(repository => repository.GetByStaffAndDateAsync(
                20, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateBookingAsync(99, ValidRequest(), CancellationToken.None));

        Assert.Equal("STAFF_NOT_WORKING", exception.Code);
    }

    [Theory]
    [InlineData("08:30", "10:00")]
    [InlineData("16:30", "17:30")]
    public async Task CreateBooking_WhenOutsideWorkingHours_ThrowsBadRequest(
        string start,
        string end)
    {
        var scheduleDate = DateOnly.FromDateTime(
            BookingTestData.FutureStart.ToOffset(TimeSpan.FromHours(7)).DateTime);
        var startTime = new DateTimeOffset(
            scheduleDate.ToDateTime(TimeOnly.Parse(start)), TimeSpan.FromHours(7));
        var expectedEnd = new DateTimeOffset(
            scheduleDate.ToDateTime(TimeOnly.Parse(end)), TimeSpan.FromHours(7));
        _services.Setup(repository => repository.GetByIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Service(durationMinutes: (int)(expectedEnd - startTime).TotalMinutes));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CreateBookingAsync(99, ValidRequest(startTime), CancellationToken.None));

        Assert.Equal("BOOKING_OUTSIDE_WORKING_HOURS", exception.Code);
    }

    [Fact]
    public async Task CreateBooking_WhenIntervalConflicts_ThrowsConflict()
    {
        _bookings.Setup(repository => repository.ExistsConflictAsync(
                20, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateBookingAsync(99, ValidRequest(), CancellationToken.None));

        Assert.Equal("BOOKING_CONFLICT", exception.Code);
    }

    [Fact]
    public async Task GetBooking_WhenCustomerRequestsAnotherCustomersBooking_ThrowsForbidden()
    {
        _bookings.Setup(repository => repository.GetByIdWithDetailsAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Booking(customerId: 2));

        var exception = await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.GetBookingAsync(30, 1, CancellationToken.None));

        Assert.Equal("BOOKING_FORBIDDEN", exception.Code);
    }

    [Fact]
    public async Task GetBooking_WhenAdminHasNoCustomerScope_ReturnsBooking()
    {
        _bookings.Setup(repository => repository.GetByIdWithDetailsAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Booking(customerId: 2));

        var response = await _sut.GetBookingAsync(30, null, CancellationToken.None);

        Assert.Equal("BK-TEST-0001", response.BookingCode);
        Assert.Equal("Haircut", response.ServiceName);
        Assert.Equal("Taylor Staff", response.StaffName);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed)]
    public async Task CancelBooking_WhenOwnerCancelsFutureBooking_SetsCancelledAndPersistsReason(
        BookingStatus status)
    {
        var booking = BookingTestData.Booking(customerId: 1, status: status);
        _bookings.Setup(repository => repository.GetByIdAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        await _sut.CancelBookingAsync(30, 1, "Schedule changed", CancellationToken.None);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal("Schedule changed", booking.CancellationReason);
        _bookings.Verify(repository => repository.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelBooking_WhenCustomerDoesNotOwnBooking_ThrowsForbidden()
    {
        _bookings.Setup(repository => repository.GetByIdAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Booking(customerId: 2));

        var exception = await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.CancelBookingAsync(30, 1, "Reason", CancellationToken.None));

        Assert.Equal("BOOKING_FORBIDDEN", exception.Code);
    }

    [Theory]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Cancelled)]
    public async Task CancelBooking_WhenBookingIsTerminal_ThrowsBadRequest(
        BookingStatus status)
    {
        _bookings.Setup(repository => repository.GetByIdAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Booking(status: status));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CancelBookingAsync(30, 1, "Reason", CancellationToken.None));

        Assert.Equal("INVALID_BOOKING_STATUS", exception.Code);
    }

    [Fact]
    public async Task CancelBooking_WhenBookingHasStarted_ThrowsBadRequest()
    {
        _bookings.Setup(repository => repository.GetByIdAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Booking(startTime: _clock.GetUtcNow().UtcDateTime));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CancelBookingAsync(30, 1, "Reason", CancellationToken.None));

        Assert.Equal("BOOKING_ALREADY_STARTED", exception.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CancelBooking_WhenReasonIsMissing_ThrowsBadRequest(string reason)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CancelBookingAsync(30, 1, reason, CancellationToken.None));

        Assert.Equal("CANCELLATION_REASON_REQUIRED", exception.Code);
        _bookings.Verify(repository => repository.GetByIdAsync(
            It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelBooking_WhenAdminScopeIsNull_CancelsBooking()
    {
        var booking = BookingTestData.Booking(customerId: 2);
        _bookings.Setup(repository => repository.GetByIdAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        await _sut.CancelBookingAsync(30, null, "Admin cancellation", CancellationToken.None);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public async Task UpdateBookingStatus_WhenPendingIsConfirmed_UpdatesStatus()
    {
        var booking = BookingTestData.Booking(status: BookingStatus.Pending);
        _bookings.Setup(repository => repository.GetByIdAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        await _sut.UpdateBookingStatusAsync(30, BookingStatus.Confirmed, CancellationToken.None);

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public async Task UpdateBookingStatus_WhenConfirmedIsCompleted_UpdatesStatus()
    {
        var booking = BookingTestData.Booking(status: BookingStatus.Confirmed);
        _bookings.Setup(repository => repository.GetByIdAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        await _sut.UpdateBookingStatusAsync(30, BookingStatus.Completed, CancellationToken.None);

        Assert.Equal(BookingStatus.Completed, booking.Status);
    }

    [Theory]
    [InlineData(BookingStatus.Pending, BookingStatus.Completed)]
    [InlineData(BookingStatus.Cancelled, BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Completed, BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Pending)]
    public async Task UpdateBookingStatus_WhenTransitionIsNotAllowed_ThrowsBadRequest(
        BookingStatus current,
        BookingStatus requested)
    {
        _bookings.Setup(repository => repository.GetByIdAsync(
                30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Booking(status: current));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateBookingStatusAsync(30, requested, CancellationToken.None));

        Assert.Equal("INVALID_BOOKING_STATUS", exception.Code);
        _bookings.Verify(repository => repository.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMyBookings_ForwardsFiltersAndMapsBookingFields()
    {
        var booking = BookingTestData.Booking(
            customerId: 77,
            status: BookingStatus.Cancelled);
        _bookings.Setup(repository => repository.GetMyBookingsAsync(
                77, new DateOnly(2035, 1, 3), BookingStatus.Cancelled, 2, 5,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Booking>)[booking], 1));
        var query = new BookingQueryParameters
        {
            Date = new DateOnly(2035, 1, 3),
            Status = BookingStatus.Cancelled,
            Page = 2,
            PageSize = 5
        };

        var result = await _sut.GetMyBookingsAsync(77, query, CancellationToken.None);
        var item = Assert.Single(result.Items);

        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal("BK-TEST-0001", item.BookingCode);
        Assert.Equal(77, item.CustomerId);
        Assert.Equal("Haircut", item.ServiceName);
        Assert.Equal("Taylor Staff", item.StaffName);
        Assert.Equal("Customer note", item.CustomerNote);
        Assert.Equal("Previously cancelled", item.CancellationReason);
        Assert.Equal(BookingStatus.Cancelled, item.Status);
        _bookings.Verify(repository => repository.GetMyBookingsAsync(
            77, query.Date, query.Status, 2, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyBookings_WhenEmpty_ReturnsEmptyPagedResult()
    {
        _bookings.Setup(repository => repository.GetMyBookingsAsync(
                77, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Booking>)Array.Empty<Booking>(), 0));

        var result = await _sut.GetMyBookingsAsync(
            77, new BookingQueryParameters(), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.TotalPages);
    }

    [Theory]
    [InlineData(0, 10, "INVALID_PAGE")]
    [InlineData(1, 0, "INVALID_PAGE_SIZE")]
    [InlineData(1, 101, "INVALID_PAGE_SIZE")]
    public async Task GetMyBookings_WhenPagingIsInvalid_ThrowsBadRequest(
        int page,
        int pageSize,
        string code)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.GetMyBookingsAsync(77,
                new BookingQueryParameters { Page = page, PageSize = pageSize },
                CancellationToken.None));

        Assert.Equal(code, exception.Code);
    }

    [Fact]
    public async Task GetBookings_ForwardsAdminFiltersAndMapsPagedResult()
    {
        var booking = BookingTestData.Booking();
        _bookings.Setup(repository => repository.GetPagedAsync(
                new DateOnly(2035, 1, 3), BookingStatus.Pending, 1, 10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Booking>)[booking], 12));

        var result = await _sut.GetBookingsAsync(new BookingQueryParameters
        {
            Date = new DateOnly(2035, 1, 3),
            Status = BookingStatus.Pending
        }, CancellationToken.None);

        Assert.Equal(12, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetAvailableSlots_ExcludesPastAndConflictingStarts()
    {
        var slotsDate = new DateOnly(2035, 1, 3);
        _services.Setup(repository => repository.GetByIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Service(durationMinutes: 60));
        _schedules.Setup(repository => repository.GetByStaffAndDateAsync(
                20, slotsDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync([BookingTestData.Schedule(new TimeOnly(9, 0), new TimeOnly(12, 0))]);
        var conflictStartUtc = new DateTime(2035, 1, 3, 3, 0, 0, DateTimeKind.Utc);
        var conflictEndUtc = new DateTime(2035, 1, 3, 4, 0, 0, DateTimeKind.Utc);
        _bookings.Setup(repository => repository.GetNonCancelledForStaffInRangeAsync(
                20, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Booking
            {
                StaffId = 20,
                StartTime = conflictStartUtc,
                EndTime = conflictEndUtc,
                Status = BookingStatus.Confirmed
            }]);

        var result = await _sut.GetAvailableSlotsAsync(new AvailableSlotsQueryParameters
        {
            ServiceId = 10,
            StaffId = 20,
            Date = slotsDate
        }, CancellationToken.None);

        Assert.DoesNotContain(result, slot => slot.StartTime.UtcDateTime == conflictStartUtc);
        Assert.Contains(result, slot => slot.StartTime.UtcDateTime == conflictEndUtc);
        Assert.DoesNotContain(result, slot => slot.StartTime.UtcDateTime < _clock.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public async Task GetAvailableSlots_WhenInactiveService_ThrowsBadRequest()
    {
        _services.Setup(repository => repository.GetByIdAsync(
                10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookingTestData.Service(isActive: false));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.GetAvailableSlotsAsync(new AvailableSlotsQueryParameters
            {
                ServiceId = 10,
                StaffId = 20,
                Date = new DateOnly(2035, 1, 3)
            }, CancellationToken.None));

        Assert.Equal("SERVICE_INACTIVE", exception.Code);
    }

    private static CreateBookingRequest ValidRequest(
        DateTimeOffset? start = null) => new()
    {
        ServiceId = 10,
        StaffId = 20,
        StartTime = start ?? BookingTestData.FutureStart,
        CustomerNote = "Please call on arrival"
    };
}
