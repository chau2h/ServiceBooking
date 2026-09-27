using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceBooking.Api.Models;

namespace ServiceBooking.Api.Data.Configurations;

public class WorkScheduleConfiguration
    : IEntityTypeConfiguration<WorkSchedule>
{
    public void Configure(EntityTypeBuilder<WorkSchedule> builder)
    {
        builder.ToTable("WorkSchedules", table =>
        {
            table.HasCheckConstraint(
                "CK_WorkSchedules_StartBeforeEnd",
                "\"StartTime\" < \"EndTime\"");
        });

        builder.HasKey(schedule => schedule.Id);

        builder.Property(schedule => schedule.Id)
            .ValueGeneratedOnAdd();

        builder.Property(schedule => schedule.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

        builder.Property(schedule => schedule.WorkDate)
            .IsRequired()
            .HasColumnName("WorkDate");

        builder.Property(schedule => schedule.StartTime)
            .IsRequired()
            .HasColumnName("StartTime");

        builder.Property(schedule => schedule.EndTime)
            .IsRequired()
            .HasColumnName("EndTime");

        builder.Property(schedule => schedule.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        // Main lookup pattern when loading a staff member's schedule for a date.
        builder.HasIndex(schedule => new
            {
                schedule.StaffId,
                schedule.WorkDate
            })
            .HasDatabaseName("IX_WorkSchedules_StaffId_WorkDate");

        builder.HasOne(schedule => schedule.Staff)
            .WithMany(staff => staff.WorkSchedules)
            .HasForeignKey(schedule => schedule.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}