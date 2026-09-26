namespace ServiceBooking.Api.Models;

public class WorkSchedule
{
    public long Id { get; set; }

    public long StaffId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public DateTime CreatedAt { get; set; }

    // Foreign key navigation.
    public Staff Staff { get; set; } = null!;
}