using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Data;

public static class DbSeeder
{
    private const string AdminEmail = "admin@servicebooking.demo";
    private const string Customer1Email = "customer1@servicebooking.demo";
    private const string Customer2Email = "customer2@servicebooking.demo";

    private const string Staff1Email = "staff1@servicebooking.demo";
    private const string Staff2Email = "staff2@servicebooking.demo";

    private const string DemoPassword = "Demo@12345";

    private static readonly IPasswordHasher<User> PasswordHasher =
    new PasswordHasher<User>();

    public static async Task SeedAsync(AppDbContext context)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var users = await SeedUsersAsync(context);
            var staffs = await SeedStaffsAsync(context);
            var services = await SeedServicesAsync(context);

            await context.SaveChangesAsync();

            await SeedWorkSchedulesAsync(context, staffs);
            await context.SaveChangesAsync();

            await SeedBookingsAsync(
                context,
                users,
                staffs,
                services);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<Dictionary<string, User>> SeedUsersAsync(
        AppDbContext context)
    {
        var userDefinitions = new[]
        {
            new
            {
                FullName = "System Administrator",
                Email = AdminEmail,
                Role = UserRole.Admin
            },
            new
            {
                FullName = "Demo Customer One",
                Email = Customer1Email,
                Role = UserRole.Customer
            },
            new
            {
                FullName = "Demo Customer Two",
                Email = Customer2Email,
                Role = UserRole.Customer
            }
        };

        var result = new Dictionary<string, User>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var definition in userDefinitions)
        {
            var user = await context.Set<User>()
                .FirstOrDefaultAsync(x => x.Email == definition.Email);

            if (user is null)
            {
                user = new User
                {
                    FullName = definition.FullName,
                    Email = definition.Email,
                    Role = definition.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Hash the demo password using the same ASP.NET Core
                // PasswordHasher that will later be used by the login flow.
                user.PasswordHash = CreatePasswordHash(
                    user,
                    DemoPassword);

                await context.Set<User>().AddAsync(user);
            }

            result[definition.Email] = user;
        }

        return result;
    }

    private static async Task<Dictionary<string, Staff>> SeedStaffsAsync(
        AppDbContext context)
    {
        var staffDefinitions = new[]
        {
            new
            {
                FullName = "Nguyen Van Staff One",
                Email = Staff1Email
            },
            new
            {
                FullName = "Tran Thi Staff Two",
                Email = Staff2Email
            }
        };

        var result = new Dictionary<string, Staff>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var definition in staffDefinitions)
        {
            var staff = await context.Set<Staff>()
                .FirstOrDefaultAsync(x => x.Email == definition.Email);

            if (staff is null)
            {
                staff = new Staff
                {
                    FullName = definition.FullName,
                    Email = definition.Email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Set<Staff>().AddAsync(staff);
            }

            result[definition.Email] = staff;
        }

        return result;
    }

    private static async Task<Dictionary<string, Service>> SeedServicesAsync(
        AppDbContext context)
    {
        var serviceDefinitions = new[]
        {
            new
            {
                Name = "Haircut",
                Description = "Basic haircut service",
                DurationMinutes = 60,
                Price = 150_000m,
                IsActive = true
            },
            new
            {
                Name = "Hair Washing",
                Description = "Hair washing and basic care",
                DurationMinutes = 30,
                Price = 80_000m,
                IsActive = true
            },
            new
            {
                Name = "Relaxing Massage",
                Description = "Full body relaxing massage",
                DurationMinutes = 60,
                Price = 300_000m,
                IsActive = true
            },
            new
            {
                Name = "Facial Treatment",
                Description = "Basic facial treatment",
                DurationMinutes = 90,
                Price = 250_000m,
                IsActive = true
            },
            new
            {
                Name = "Hair Coloring",
                Description = "Professional hair coloring service",
                DurationMinutes = 120,
                Price = 500_000m,
                IsActive = false
            }
        };

        var result = new Dictionary<string, Service>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var definition in serviceDefinitions)
        {
            var service = await context.Set<Service>()
                .FirstOrDefaultAsync(x => x.Name == definition.Name);

            if (service is null)
            {
                service = new Service
                {
                    Name = definition.Name,
                    Description = definition.Description,
                    DurationMinutes = definition.DurationMinutes,
                    Price = definition.Price,
                    IsActive = definition.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Set<Service>().AddAsync(service);
            }

            result[definition.Name] = service;
        }

        return result;
    }

    private static async Task SeedWorkSchedulesAsync(
        AppDbContext context,
        Dictionary<string, Staff> staffs)
    {
        var baseDate = DateOnly.FromDateTime(DateTime.Now);

        // The requirement asks for work schedules covering 7 days.
        // We create one 09:00-17:00 shift per staff for each day.
        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var workDate = baseDate.AddDays(dayOffset);

            foreach (var staff in staffs.Values)
            {
                var exists = await context.Set<WorkSchedule>()
                    .AnyAsync(schedule =>
                        schedule.StaffId == staff.Id &&
                        schedule.WorkDate == workDate &&
                        schedule.StartTime == new TimeOnly(9, 0) &&
                        schedule.EndTime == new TimeOnly(17, 0));

                if (exists)
                {
                    continue;
                }

                var schedule = new WorkSchedule
                {
                    StaffId = staff.Id,
                    WorkDate = workDate,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0),
                    CreatedAt = DateTime.UtcNow
                };

                await context.Set<WorkSchedule>().AddAsync(schedule);
            }
        }
    }

    private static async Task SeedBookingsAsync(
        AppDbContext context,
        Dictionary<string, User> users,
        Dictionary<string, Staff> staffs,
        Dictionary<string, Service> services)
    {
        var baseDate = DateOnly.FromDateTime(DateTime.Now);

        var customer1 = users[Customer1Email];
        var customer2 = users[Customer2Email];

        var staff1 = staffs[Staff1Email];
        var staff2 = staffs[Staff2Email];

        var haircut = services["Haircut"];
        var hairWashing = services["Hair Washing"];
        var massage = services["Relaxing Massage"];
        var facial = services["Facial Treatment"];

        var bookingDefinitions = new[]
        {
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-001",
                CustomerId = customer1.Id,
                StaffId = staff1.Id,
                ServiceId = haircut.Id,
                Date = baseDate.AddDays(-1),
                Start = new TimeOnly(9, 0),
                Status = BookingStatus.Completed,
                CustomerNote = "Completed demo booking"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-002",
                CustomerId = customer2.Id,
                StaffId = staff1.Id,
                ServiceId = hairWashing.Id,
                Date = baseDate.AddDays(-1),
                Start = new TimeOnly(10, 0),
                Status = BookingStatus.Cancelled,
                CustomerNote = "Cancelled demo booking",
                CancellationReason = "Customer changed schedule"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-003",
                CustomerId = customer1.Id,
                StaffId = staff1.Id,
                ServiceId = haircut.Id,
                Date = baseDate.AddDays(1),
                Start = new TimeOnly(9, 0),
                Status = BookingStatus.Confirmed,
                CustomerNote = "Confirmed demo booking"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-004",
                CustomerId = customer2.Id,
                StaffId = staff1.Id,
                ServiceId = massage.Id,
                Date = baseDate.AddDays(1),
                Start = new TimeOnly(10, 30),
                Status = BookingStatus.Pending,
                CustomerNote = "Pending demo booking"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-005",
                CustomerId = customer1.Id,
                StaffId = staff2.Id,
                ServiceId = facial.Id,
                Date = baseDate.AddDays(1),
                Start = new TimeOnly(13, 0),
                Status = BookingStatus.Confirmed,
                CustomerNote = "Facial treatment booking"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-006",
                CustomerId = customer2.Id,
                StaffId = staff1.Id,
                ServiceId = massage.Id,
                Date = baseDate.AddDays(2),
                Start = new TimeOnly(9, 0),
                Status = BookingStatus.Pending,
                CustomerNote = "Morning massage booking"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-007",
                CustomerId = customer1.Id,
                StaffId = staff2.Id,
                ServiceId = hairWashing.Id,
                Date = baseDate.AddDays(2),
                Start = new TimeOnly(11, 0),
                Status = BookingStatus.Confirmed,
                CustomerNote = "Hair washing booking"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-008",
                CustomerId = customer2.Id,
                StaffId = staff2.Id,
                ServiceId = haircut.Id,
                Date = baseDate.AddDays(2),
                Start = new TimeOnly(14, 0),
                Status = BookingStatus.Cancelled,
                CustomerNote = "Cancelled afternoon booking",
                CancellationReason = "Customer unavailable"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-009",
                CustomerId = customer1.Id,
                StaffId = staff1.Id,
                ServiceId = facial.Id,
                Date = baseDate.AddDays(3),
                Start = new TimeOnly(13, 0),
                Status = BookingStatus.Pending,
                CustomerNote = "Future facial treatment"
            },
            new BookingSeedDefinition
            {
                BookingCode = "DEMO-BK-010",
                CustomerId = customer2.Id,
                StaffId = staff2.Id,
                ServiceId = massage.Id,
                Date = baseDate.AddDays(4),
                Start = new TimeOnly(15, 0),
                Status = BookingStatus.Confirmed,
                CustomerNote = "Future massage booking"
            }
        };

        foreach (var definition in bookingDefinitions)
        {
            var exists = await context.Set<Booking>()
                .AnyAsync(x => x.BookingCode == definition.BookingCode);

            if (exists)
            {
                continue;
            }

            // Booking time is stored as UTC.
            // The seed input represents business-local time in UTC+07:00.
            var startTimeUtc = ToUtc(
                definition.Date,
                definition.Start);

            var endTimeUtc = startTimeUtc.AddMinutes(
                GetDurationMinutes(
                    services,
                    definition.ServiceId));

            var booking = new Booking
            {
                BookingCode = definition.BookingCode,
                CustomerId = definition.CustomerId,
                ServiceId = definition.ServiceId,
                StaffId = definition.StaffId,
                StartTime = startTimeUtc,
                EndTime = endTimeUtc,
                Status = definition.Status,
                CustomerNote = definition.CustomerNote,
                CancellationReason = definition.CancellationReason,
                CreatedAt = startTimeUtc.AddDays(-2)
            };

            await context.Set<Booking>().AddAsync(booking);
        }
    }

    private static int GetDurationMinutes(
        Dictionary<string, Service> services,
        long serviceId)
    {
        var service = services.Values.FirstOrDefault(
            x => x.Id == serviceId);

        if (service is null)
        {
            throw new InvalidOperationException(
                $"Service with Id {serviceId} was not found.");
        }

        return service.DurationMinutes;
    }

    private static DateTime ToUtc(
        DateOnly date,
        TimeOnly time)
    {
        var localDateTime = date.ToDateTime(
            time,
            DateTimeKind.Unspecified);

        var vietnamOffset = TimeSpan.FromHours(7);

        return new DateTimeOffset(
                localDateTime,
                vietnamOffset)
            .UtcDateTime;
    }

    private static string CreatePasswordHash(
        User user,
        string password)
    {
        return PasswordHasher.HashPassword(
            user,
            password);
    }

    private sealed class BookingSeedDefinition
    {
        public required string BookingCode { get; init; }

        public long CustomerId { get; init; }

        public long StaffId { get; init; }

        public long ServiceId { get; init; }

        public DateOnly Date { get; init; }

        public TimeOnly Start { get; init; }

        public BookingStatus Status { get; init; }

        public string? CustomerNote { get; init; }

        public string? CancellationReason { get; init; }
    }
}