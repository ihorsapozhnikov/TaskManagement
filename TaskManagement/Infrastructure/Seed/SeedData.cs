using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain;

namespace TaskManagement.Infrastructure.Seed;

public static class SeedData
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        SeedEmployees(modelBuilder);
        SeedTasks(modelBuilder);
    }
    private static void SeedEmployees(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Id = 1,
                FullName = "Tom Johnson",
                IsActive = true
            },
            new Employee
            {
                Id = 2,
                FullName = "David Tyson",
                IsActive = true
            },
            new Employee
            {
                Id = 3,
                FullName = "Mike Fischer",
                IsActive = false
            }
        );
    }
    private static void SeedTasks(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = 1,
                Title = "Prepare monthly report",
                Description = "Prepare the monthly sales report.",
                PlannedStartAt = new DateTime(2026, 9, 15, 9, 0, 0, DateTimeKind.Utc),
                DueAt = new DateTime(2026, 9, 20, 18, 0, 0, DateTimeKind.Utc),
                Status = Domain.TaskStatus.New,
                CompletedAt = null,
                Version = 1,
                CreatedByEmployeeId = 1,
                AssigneeId = 2
            },
            new TaskItem
            {
                Id = 2,
                Title = "Update documentation",
                Description = "Update internal project documentation.",
                PlannedStartAt = new DateTime(2026, 9, 10, 9, 0, 0, DateTimeKind.Utc),
                DueAt = new DateTime(2026, 9, 18, 18, 0, 0, DateTimeKind.Utc),
                Status = Domain.TaskStatus.InProgress,
                CompletedAt = null,
                Version = 1,
                CreatedByEmployeeId = 2,
                AssigneeId = 1
            },
            new TaskItem
            {
                Id = 3,
                Title = "Prepare presentation",
                Description = "Prepare presentation for the team meeting.",
                PlannedStartAt = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc),
                DueAt = new DateTime(2026, 9, 5, 18, 0, 0, DateTimeKind.Utc),
                Status = Domain.TaskStatus.Completed,
                CompletedAt = new DateTime(2026, 9, 4, 15, 30, 0, DateTimeKind.Utc),
                Version = 1,
                CreatedByEmployeeId = 1,
                AssigneeId = 2
            }
        );
    }
}