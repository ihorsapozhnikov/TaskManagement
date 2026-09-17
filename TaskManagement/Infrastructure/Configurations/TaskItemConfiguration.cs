using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain;

namespace TaskManagement.Infrastructure.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.PlannedStartAt)
            .IsRequired();

        builder.Property(t => t.DueAt)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.CompletedAt)
            .IsRequired(false);

        builder.HasOne(t => t.CreatedByEmployee)
            .WithMany(e => e.CreatedTasks)
            .HasForeignKey(t => t.CreatedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Assignee)
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.AssigneeId);

        builder.ToTable("Tasks", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Tasks_Status_Valid",
                "\"Status\" IN (0, 1, 2, 3)");

            tableBuilder.HasCheckConstraint(
                "CK_Tasks_DueAt_GreaterThanOrEqual_PlannedStartAt",
                "\"DueAt\" >= \"PlannedStartAt\"");

            tableBuilder.HasCheckConstraint(
                "CK_Tasks_Creator_Different_From_Assignee",
                "\"CreatedByEmployeeId\" <> \"AssigneeId\"");

            tableBuilder.HasCheckConstraint(
                "CK_Tasks_CompletedAt_Consistent_With_Status",
                "(\"Status\" = 2 AND \"CompletedAt\" IS NOT NULL) OR " +
                "(\"Status\" <> 2 AND \"CompletedAt\" IS NULL)");
        });
    }
}