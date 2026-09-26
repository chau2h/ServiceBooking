namespace ServiceBooking.Api.Models;

public class Staff
{
    public long Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // A staff member can have multiple work schedules.
    public ICollection<WorkSchedule> WorkSchedules { get; set; } =
        new List<WorkSchedule>();

    // A staff member can handle multiple bookings.
    public ICollection<Booking> Bookings { get; set; } =
        new List<Booking>();
}