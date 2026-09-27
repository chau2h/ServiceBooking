using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceBooking.Api.Common.Enums;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Data.Configurations;

public class BookingConfiguration
    : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.Id)
            .ValueGeneratedOnAdd();

        builder.Property(booking => booking.BookingCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("BookingCode");

        builder.Property(booking => booking.CustomerId)
            .IsRequired()
            .HasColumnName("CustomerId");

        builder.Property(booking => booking.ServiceId)
            .IsRequired()
            .HasColumnName("ServiceId");

        builder.Property(booking => booking.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

        builder.Property(booking => booking.StartTime)
            .IsRequired()
            .HasColumnName("StartTime");

        builder.Property(booking => booking.EndTime)
            .IsRequired()
            .HasColumnName("EndTime");

        builder.Property(booking => booking.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("Status");

        builder.Property(booking => booking.CustomerNote)
            .HasMaxLength(1000)
            .HasColumnName("CustomerNote");

        builder.Property(booking => booking.CancellationReason)
            .HasMaxLength(500)
            .HasColumnName("CancellationReason");

        builder.Property(booking => booking.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        // Booking.CustomerId -> Users.Id
        builder.HasOne(booking => booking.Customer)
            .WithMany(user => user.Bookings)
            .HasForeignKey(booking => booking.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking.ServiceId -> Services.Id
        builder.HasOne(booking => booking.Service)
            .WithMany(service => service.Bookings)
            .HasForeignKey(booking => booking.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking.StaffId -> Staffs.Id
        builder.HasOne(booking => booking.Staff)
            .WithMany(staff => staff.Bookings)
            .HasForeignKey(booking => booking.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}