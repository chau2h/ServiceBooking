using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedOnAdd();

        builder.Property(user => user.FullName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("FullName");

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("Email");

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("PasswordHash");

        builder.Property(user => user.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("Role");

        builder.Property(user => user.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("IsActive");

        builder.Property(user => user.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        // A user can create many bookings.
        // CustomerId in Bookings references Users.Id.
        builder.HasMany(user => user.Bookings)
            .WithOne(booking => booking.Customer)
            .HasForeignKey(booking => booking.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}