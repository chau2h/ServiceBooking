using Microsoft.AspNetCore.Identity;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Authentication
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IWorkScheduleRepository, WorkScheduleRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IServiceManagementService, ServiceManagementService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}