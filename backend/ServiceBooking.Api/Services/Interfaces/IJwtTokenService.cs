using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(User user);
}