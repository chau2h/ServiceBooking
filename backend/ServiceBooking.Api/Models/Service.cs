namespace ServiceBooking.Api.Models;

public class Service
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation property:
    // A service can be selected in many bookings.
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}