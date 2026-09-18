namespace TaskManagement.Domain;
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime PlannedStartAt { get; set; }
    public DateTime DueAt { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long Version { get; set; }
    public int CreatedByEmployeeId { get; set; }
    public int AssigneeId { get; set; }
    public Employee CreatedByEmployee { get; set; } = null!;
    public Employee Assignee { get; set; } = null!;
}