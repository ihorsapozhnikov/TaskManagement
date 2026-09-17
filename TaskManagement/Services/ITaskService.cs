using TaskManagement.Domain;

namespace TaskManagement.Services;

public interface ITaskService
{
    TaskItem CreateTask(string title, string? description, DateTime plannedStartAt, DateTime dueAt, int createdByEmployeeId, int assigneeId);
    void ChangeStatus(int taskId, Domain.TaskStatus newStatus);
    IReadOnlyList<TaskItem> GetTasksByAssignee(int assigneeId);
}