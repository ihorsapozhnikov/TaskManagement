using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain;
using TaskManagement.Infrastructure;

namespace TaskManagement.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;

    public TaskService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public TaskItem CreateTask(string title, string? description, DateTime plannedStartAt, DateTime dueAt, int createdByEmployeeId, int assigneeId)
    {
        var assignee = _dbContext.Employees.SingleOrDefault(e => e.Id == assigneeId);

        if (assignee is null)
        {
            throw new InvalidOperationException("Assignee not found.");
        }

        if (!assignee.IsActive)
        {
            throw new InvalidOperationException("Assignee is inactive.");
        }

        var creator = _dbContext.Employees.SingleOrDefault(e => e.Id == createdByEmployeeId);

        if (creator is null)
        {
            throw new InvalidOperationException("Creator not found.");
        }

        if (createdByEmployeeId == assigneeId)
        {
            throw new InvalidOperationException("Task creator cannot be the assignee.");
        }

        if (dueAt < plannedStartAt)
        {
            throw new InvalidOperationException("Due date cannot be earlier than planned start date.");
        }

        var task = new TaskItem
        {
            Title = title,
            Description = description,
            PlannedStartAt = plannedStartAt,
            DueAt = dueAt,
            Status = Domain.TaskStatus.New,
            CompletedAt = null,
            CreatedByEmployeeId = createdByEmployeeId,
            AssigneeId = assigneeId
        };

        _dbContext.Tasks.Add(task);
        _dbContext.SaveChanges();

        return task;
    }

    public void ChangeStatus(int taskId, Domain.TaskStatus newStatus)
    {
        var task = _dbContext.Tasks.SingleOrDefault(t => t.Id == taskId);

        if (task is null)
        {
            throw new InvalidOperationException("Task not found.");
        }

        if (task.Status == Domain.TaskStatus.New &&
            (newStatus == Domain.TaskStatus.InProgress || newStatus == Domain.TaskStatus.Cancelled))
        {
            task.Status = newStatus;
            _dbContext.SaveChanges();
            return;
        }

        if (task.Status == Domain.TaskStatus.InProgress &&
            (newStatus == Domain.TaskStatus.Completed || newStatus == Domain.TaskStatus.Cancelled))
        {
            task.Status = newStatus;

            if (newStatus == Domain.TaskStatus.Completed)
            {
                task.CompletedAt = DateTime.UtcNow;
            }

            _dbContext.SaveChanges();
            return;
        }

        throw new InvalidOperationException("Invalid status transition.");
    }

    public IReadOnlyList<TaskItem> GetTasksByAssignee(int assigneeId)
    {
        throw new NotImplementedException();
    }
}