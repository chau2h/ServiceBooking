using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceBooking.Api.Common.Options;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.Extensions;
using ServiceBooking.Api.Middleware;
using ServiceBooking.Api.Common.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found."
            )
    ));

// JWT configuration
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.SecretKey),
        "Jwt:SecretKey must be configured.")
    .Validate(options =>
        options.SecretKey.Length >= 32,
        "Jwt:SecretKey must be at least 32 characters.")
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Issuer),
        "Jwt:Issuer must be configured.")
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Audience),
        "Jwt:Audience must be configured.")
    .Validate(options =>
        options.ExpirationMinutes > 0,
        "Jwt:ExpirationMinutes must be greater than zero.")
    .ValidateOnStart();

var jwtConfiguration =
    builder.Configuration.GetSection(JwtOptions.SectionName);

var jwtSecretKey =
    jwtConfiguration["SecretKey"]
    ?? throw new InvalidOperationException(
        "Jwt:SecretKey is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtConfiguration["Issuer"],

                ValidateAudience = true,
                ValidAudience = jwtConfiguration["Audience"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecretKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.Authenticated,
        policy =>
        {
            policy.RequireAuthenticatedUser();
        });

    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy =>
        {
            policy.RequireRole(Roles.Admin);
        });
});

builder.Services.AddApplicationServices();

var app = builder.Build();

// Centralized exception handling for the entire API.
app.UseMiddleware<GlobalExceptionMiddleware>();

// Authentication must run before Authorization.
app.UseAuthentication();
app.UseAuthorization();

// Seed demo data.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();