using TaskManagement.Domain;

namespace TaskManagement.Services;

/// <summary>
/// Provides operations for managing tasks.
/// All task timestamps use UTC.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Creates a new task using UTC timestamps.
    /// </summary>
    /// <param name="title">Task title.</param>
    /// <param name="description">Optional task description.</param>
    /// <param name="plannedStartAt">Task start time. Must be UTC.</param>
    /// <param name="dueAt">Task due time. Must be UTC.</param>
    /// <param name="createdByEmployeeId">ID of the employee creating the task.</param>
    /// <param name="assigneeId">ID of the employee assigned to the task.</param>
    /// <returns>The created task.</returns>
    /// <exception cref="ArgumentException">Thrown when a timestamp is not UTC.</exception>
    TaskItem CreateTask(string title, string? description, DateTime plannedStartAt, DateTime dueAt, int createdByEmployeeId, int assigneeId);

    /// <summary>
    /// Changes the status of a task.
    /// </summary>
    /// <param name="taskId">ID of the task.</param>
    /// <param name="newStatus">The new task status.</param>
    void ChangeStatus(int taskId, Domain.TaskStatus newStatus);

    /// <summary>
    /// Returns all tasks assigned to the specified employee.
    /// </summary>
    /// <param name="assigneeId">ID of the assignee.</param>
    /// <returns>A read-only list of assigned tasks.</returns>
    IReadOnlyList<TaskItem> GetTasksByAssignee(int assigneeId);
}