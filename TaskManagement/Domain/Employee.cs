namespace TaskManagement.Domain;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public ICollection<TaskItem> CreatedTasks { get; set; } = [];
    public ICollection<TaskItem> AssignedTasks { get; set; } = [];
}