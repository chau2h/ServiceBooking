using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Staffs;

public sealed class UpdateStaffRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}