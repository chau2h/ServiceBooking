using ServiceBooking.Api.Common.Enums;

namespace ServiceBooking.Api.Models;

public class User
{
    public long Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // Navigation property:
    // A user can create multiple bookings.
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}