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

        // Staff email must be unique inside the Staffs table.
        builder.HasIndex(staff => staff.Email)
            .IsUnique()
            .HasDatabaseName("UX_Staffs_Email");

        builder.HasMany(staff => staff.WorkSchedules)
            .WithOne(schedule => schedule.Staff)
            .HasForeignKey(schedule => schedule.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(staff => staff.Bookings)
            .WithOne(booking => booking.Staff)
            .HasForeignKey(booking => booking.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}