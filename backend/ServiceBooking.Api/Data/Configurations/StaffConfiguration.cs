using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staffs");

        builder.HasKey(staff => staff.Id);

        builder.Property(staff => staff.Id)
            .ValueGeneratedOnAdd();

        builder.Property(staff => staff.FullName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("FullName");

        builder.Property(staff => staff.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("Email");

        builder.Property(staff => staff.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("IsActive");

        builder.Property(staff => staff.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(staff => staff.UpdatedAt)
            .IsRequired()
            .HasColumnName("UpdatedAt");

        // A staff member can have many work schedules.
        builder.HasMany(staff => staff.WorkSchedules)
            .WithOne(schedule => schedule.Staff)
            .HasForeignKey(schedule => schedule.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // A staff member can handle many bookings.
        builder.HasMany(staff => staff.Bookings)
            .WithOne(booking => booking.Staff)
            .HasForeignKey(booking => booking.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}