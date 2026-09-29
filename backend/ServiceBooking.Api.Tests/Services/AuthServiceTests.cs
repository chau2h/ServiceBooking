using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Common.Exceptions;
using ServiceBooking.Api.Common.Options;
using ServiceBooking.Api.DTOs.Auth;
using ServiceBooking.Api.Models;
using ServiceBooking.Api.Repositories.Interfaces;
using ServiceBooking.Api.Services;
using ServiceBooking.Api.Services.Interfaces;
using Xunit;

namespace ServiceBooking.Api.Tests.Services;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher<User>> _hasher = new();
    private readonly Mock<IJwtTokenService> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _users.Object,
            _hasher.Object,
            _tokens.Object,
            Options.Create(new JwtOptions { ExpirationMinutes = 20 }));
        _tokens.Setup(service => service.CreateToken(It.IsAny<User>()))
            .Returns("token-value");
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsSafeUserAndToken()
    {
        var user = ValidUser();
        _users.Setup(repository => repository.GetByEmailAsync(
                "user@example.test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(hasher => hasher.VerifyHashedPassword(
                user, "hashed-secret", "password"))
            .Returns(PasswordVerificationResult.Success);

        var response = await _sut.LoginAsync(new LoginRequest
        {
            Email = " user@example.test ",
            Password = "password"
        });

        Assert.Equal("token-value", response.AccessToken);
        Assert.Equal(1200, response.ExpiresIn);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal(user.FullName, response.User.FullName);
        Assert.Equal(user.Email, response.User.Email);
        Assert.Equal("Customer", response.User.Role);
        Assert.DoesNotContain("PasswordHash", string.Join(',', response.User.GetType().GetProperties().Select(property => property.Name)));
        _tokens.Verify(service => service.CreateToken(user), Times.Once);
    }

    [Fact]
    public async Task Login_WhenUserDoesNotExist_UsesGenericInvalidCredentials()
    {
        _users.Setup(repository => repository.GetByEmailAsync(
                "missing@example.test", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest
            {
                Email = "missing@example.test",
                Password = "password"
            }));

        Assert.Equal("INVALID_CREDENTIALS", exception.Code);
        _hasher.Verify(hasher => hasher.VerifyHashedPassword(
            It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_UsesGenericInvalidCredentials()
    {
        var user = ValidUser();
        _users.Setup(repository => repository.GetByEmailAsync(
                "user@example.test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(hasher => hasher.VerifyHashedPassword(
                user, user.PasswordHash, "wrong"))
            .Returns(PasswordVerificationResult.Failed);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest
            {
                Email = user.Email,
                Password = "wrong"
            }));

        Assert.Equal("INVALID_CREDENTIALS", exception.Code);
        _tokens.Verify(service => service.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_WhenUserIsInactive_UsesGenericInvalidCredentials()
    {
        var user = ValidUser();
        user.IsActive = false;
        _users.Setup(repository => repository.GetByEmailAsync(
                "user@example.test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest
            {
                Email = user.Email,
                Password = "password"
            }));

        Assert.Equal("INVALID_CREDENTIALS", exception.Code);
        _hasher.Verify(hasher => hasher.VerifyHashedPassword(
            It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserIsInactive_ThrowsUnauthorized()
    {
        var user = ValidUser();
        user.IsActive = false;
        _users.Setup(repository => repository.GetByIdAsync(
                user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.GetCurrentUserAsync(user.Id));

        Assert.Equal("AUTHENTICATED_USER_NOT_FOUND", exception.Code);
    }

    [Fact]
    public async Task GetCurrentUser_WhenActive_ReturnsProfileWithoutPasswordHash()
    {
        var user = ValidUser();
        _users.Setup(repository => repository.GetByIdAsync(
                user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetCurrentUserAsync(user.Id);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.FullName, result.FullName);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal("Customer", result.Role);
        Assert.DoesNotContain("PasswordHash", result.GetType().GetProperties()
            .Select(property => property.Name));
    }

    private static User ValidUser() => new()
    {
        Id = 8,
        FullName = "Customer",
        Email = "user@example.test",
        PasswordHash = "hashed-secret",
        Role = UserRole.Customer,
        IsActive = true
    };
}
