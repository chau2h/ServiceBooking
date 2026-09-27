using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(service => service.Id);

        builder.Property(service => service.Id)
            .ValueGeneratedOnAdd();

        builder.Property(service => service.Name)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("Name");

        builder.Property(service => service.Description)
            .HasMaxLength(1000)
            .HasColumnName("Description");

        builder.Property(service => service.DurationMinutes)
            .IsRequired()
            .HasColumnName("DurationMinutes");

        builder.Property(service => service.Price)
            .IsRequired()
            .HasPrecision(12, 2)
            .HasColumnName("Price");

        builder.Property(service => service.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("IsActive");

        builder.Property(service => service.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(service => service.UpdatedAt)
            .IsRequired()
            .HasColumnName("UpdatedAt");

        // A service can be used by many bookings.
        builder.HasMany(service => service.Bookings)
            .WithOne(booking => booking.Service)
            .HasForeignKey(booking => booking.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}