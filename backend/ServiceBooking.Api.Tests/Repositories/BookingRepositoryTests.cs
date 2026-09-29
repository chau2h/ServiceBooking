using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories;
using Xunit;

namespace ServiceBooking.Api.Tests.Repositories;

public sealed class BookingRepositoryTests
{
    [Theory]
    [InlineData("09:30", "09:45", BookingStatus.Pending, 20, true)]
    [InlineData("08:30", "09:30", BookingStatus.Confirmed, 20, true)]
    [InlineData("09:30", "10:30", BookingStatus.Completed, 20, true)]
    [InlineData("08:00", "11:00", BookingStatus.Pending, 20, true)]
    [InlineData("09:00", "10:00", BookingStatus.Confirmed, 20, true)]
    [InlineData("08:00", "10:00", BookingStatus.Pending, 20, true)]
    [InlineData("09:00", "11:00", BookingStatus.Pending, 20, true)]
    [InlineData("10:00", "11:00", BookingStatus.Pending, 20, false)]
    [InlineData("08:00", "09:00", BookingStatus.Pending, 20, false)]
    [InlineData("09:30", "09:45", BookingStatus.Pending, 21, false)]
    [InlineData("09:30", "09:45", BookingStatus.Cancelled, 20, false)]
    public async Task ExistsConflict_UsesStrictOverlapAndExcludesCancelledBookings(
        string newStart,
        string newEnd,
        BookingStatus existingStatus,
        long newStaffId,
        bool expected)
    {
        await using var fixture = await RepositoryFixture.CreateAsync(
            existingStatus,
            existingStaffId: 20);

        var result = await fixture.Repository.ExistsConflictAsync(
            newStaffId,
            Utc(newStart),
            Utc(newEnd),
            CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetMyBookings_FiltersByCustomerStatusAndBusinessDateBeforePaging()
    {
        await using var fixture = await RepositoryFixture.CreateAsync(
            BookingStatus.Pending,
            existingStaffId: 20);
        await fixture.AddBookingAsync(
            fixture.CustomerId,
            "BK-MY-002",
            BookingStatus.Pending,
            Utc("11:00"));
        await fixture.AddBookingAsync(
            fixture.OtherCustomerId,
            "BK-MY-003",
            BookingStatus.Pending,
            Utc("12:00"));
        await fixture.AddBookingAsync(
            fixture.CustomerId,
            "BK-MY-004",
            BookingStatus.Confirmed,
            Utc("13:00"));

        var firstPage = await fixture.Repository.GetMyBookingsAsync(
            fixture.CustomerId,
            new DateOnly(2035, 1, 3),
            BookingStatus.Pending,
            1,
            1,
            CancellationToken.None);
        var secondPage = await fixture.Repository.GetMyBookingsAsync(
            fixture.CustomerId,
            new DateOnly(2035, 1, 3),
            BookingStatus.Pending,
            2,
            1,
            CancellationToken.None);

        Assert.Equal(2, firstPage.TotalCount);
        Assert.Equal(2, secondPage.TotalCount);
        Assert.Single(firstPage.Items);
        Assert.Single(secondPage.Items);
        Assert.NotEqual(firstPage.Items[0].Id, secondPage.Items[0].Id);
        Assert.All(firstPage.Items.Concat(secondPage.Items), booking =>
        {
            Assert.Equal(fixture.CustomerId, booking.CustomerId);
            Assert.Equal(BookingStatus.Pending, booking.Status);
        });
    }

    private static DateTime Utc(string time) =>
        DateTime.SpecifyKind(
            new DateOnly(2035, 1, 3)
                .ToDateTime(TimeOnly.Parse(time)),
            DateTimeKind.Utc);

    private sealed class RepositoryFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;

        private RepositoryFixture(
            SqliteConnection connection,
            AppDbContext context,
            BookingRepository repository,
            long customerId,
            long otherCustomerId)
        {
            _connection = connection;
            _context = context;
            Repository = repository;
            CustomerId = customerId;
            OtherCustomerId = otherCustomerId;
        }

        public BookingRepository Repository { get; }

        public long CustomerId { get; }

        public long OtherCustomerId { get; }

        public static async Task<RepositoryFixture> CreateAsync(
            BookingStatus status,
            long existingStaffId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var customer = new User
            {
                FullName = "Customer",
                Email = "customer@example.test",
                PasswordHash = "hash",
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            };
            var otherCustomer = new User
            {
                FullName = "Other customer",
                Email = "other-customer@example.test",
                PasswordHash = "hash",
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            };
            var service = new Service
            {
                Name = "Test service",
                DurationMinutes = 60,
                Price = 10m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var staff = new Staff
            {
                Id = 20,
                FullName = "Test staff",
                Email = "staff@example.test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.AddRange(customer, otherCustomer, service, staff);
            await context.SaveChangesAsync();

            context.Set<Booking>().Add(new Booking
            {
                BookingCode = "BK-REPOSITORY-TEST",
                CustomerId = customer.Id,
                ServiceId = service.Id,
                StaffId = existingStaffId,
                StartTime = Utc("09:00"),
                EndTime = Utc("10:00"),
                Status = status,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            return new RepositoryFixture(
                connection,
                context,
                new BookingRepository(context),
                customer.Id,
                otherCustomer.Id);
        }

        public async Task AddBookingAsync(
            long customerId,
            string bookingCode,
            BookingStatus status,
            DateTime startUtc)
        {
            _context.Set<Booking>().Add(new Booking
            {
                BookingCode = bookingCode,
                CustomerId = customerId,
                ServiceId = 1,
                StaffId = 20,
                StartTime = startUtc,
                EndTime = startUtc.AddMinutes(30),
                Status = status,
                CreatedAt = startUtc.AddDays(-1)
            });

            await _context.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
