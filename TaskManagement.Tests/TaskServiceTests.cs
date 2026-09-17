using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain;
using TaskManagement.Infrastructure;
using TaskManagement.Services;

namespace TaskManagement.Tests;

public class TaskServiceTests
{
    private static readonly DateTime PlannedStartAt = new(2026, 9, 17, 10, 0, 0);
    private static readonly DateTime DueAt = new(2026, 9, 17, 18, 0, 0);
    private static readonly DateTime InvalidPlannedStartAt = new(2026, 9, 17, 18, 0, 0);
    private static readonly DateTime InvalidDueAt = new(2026, 9, 17, 10, 0, 0);

    private static TaskService CreateTaskService(out AppDbContext dbContext)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        dbContext = new AppDbContext(options);
        dbContext.Database.EnsureCreated();

        return new TaskService(dbContext);
    }

    [Fact]
    public void CreateTask_InactiveAssignee_Throws()
    {
        var taskService = CreateTaskService(out _);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            taskService.CreateTask(
                title: "Test task",
                description: "Test description",
                plannedStartAt: PlannedStartAt,
                dueAt: DueAt,
                createdByEmployeeId: 1,
                assigneeId: 3));

        Assert.Equal("Assignee is inactive.", exception.Message);
    }

    [Fact]
    public void CreateTask_CreatorIsAssignee_Throws()
    {
        var taskService = CreateTaskService(out _);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            taskService.CreateTask(
                title: "Test task",
                description: "Test description",
                plannedStartAt: PlannedStartAt,
                dueAt: DueAt,
                createdByEmployeeId: 1,
                assigneeId: 1));

        Assert.Equal("Task creator cannot be the assignee.", exception.Message);
    }

    [Fact]
    public void CreateTask_DueDateEarlierThanPlannedStart_Throws()
    {
        var taskService = CreateTaskService(out _);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            taskService.CreateTask(
                title: "Test task",
                description: "Test description",
                plannedStartAt: InvalidPlannedStartAt,
                dueAt: InvalidDueAt,
                createdByEmployeeId: 1,
                assigneeId: 2));

        Assert.Equal("Due date cannot be earlier than planned start date.", exception.Message);
    }

    [Fact]
    public void CreateTask_ValidData_CreatesTask()
    {
        var taskService = CreateTaskService(out var dbContext);

        var task = taskService.CreateTask(
            title: "New test task",
            description: "Test description",
            plannedStartAt: PlannedStartAt,
            dueAt: DueAt,
            createdByEmployeeId: 1,
            assigneeId: 2);

        Assert.NotEqual(0, task.Id);
        Assert.Equal("New test task", task.Title);
        Assert.Equal("Test description", task.Description);
        Assert.Equal(PlannedStartAt, task.PlannedStartAt);
        Assert.Equal(DueAt, task.DueAt);
        Assert.Equal(Domain.TaskStatus.New, task.Status);
        Assert.Null(task.CompletedAt);
        Assert.Equal(1, task.CreatedByEmployeeId);
        Assert.Equal(2, task.AssigneeId);

        var savedTask = dbContext.Tasks.Single(t => t.Id == task.Id);

        Assert.Equal(task.Id, savedTask.Id);
        Assert.Equal(task.Title, savedTask.Title);
        Assert.Equal(task.Description, savedTask.Description);
        Assert.Equal(task.Status, savedTask.Status);
        Assert.Equal(task.CreatedByEmployeeId, savedTask.CreatedByEmployeeId);
        Assert.Equal(task.AssigneeId, savedTask.AssigneeId);
        Assert.Null(savedTask.CompletedAt);
    }

    [Fact]
    public void ChangeStatus_NewToInProgress_ChangesStatus()
    {
        var taskService = CreateTaskService(out var dbContext);
        taskService.ChangeStatus(1, Domain.TaskStatus.InProgress);
        var task = dbContext.Tasks.Single(t => t.Id == 1);

        Assert.Equal(Domain.TaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void ChangeStatus_NewToCancelled_ChangesStatus()
    {
        var taskService = CreateTaskService(out var dbContext);
        taskService.ChangeStatus(1, Domain.TaskStatus.Cancelled);
        var task = dbContext.Tasks.Single(t => t.Id == 1);

        Assert.Equal(Domain.TaskStatus.Cancelled, task.Status);
    }

    [Fact]
    public void ChangeStatus_InProgressToCompleted_ChangesStatusAndSetsCompletedAt()
    {
        var taskService = CreateTaskService(out var dbContext);
        taskService.ChangeStatus(2, Domain.TaskStatus.Completed);
        var task = dbContext.Tasks.Single(t => t.Id == 2);

        Assert.Equal(Domain.TaskStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
        Assert.True(task.CompletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void ChangeStatus_InProgressToCancelled_ChangesStatus()
    {
        var taskService = CreateTaskService(out var dbContext);
        taskService.ChangeStatus(2, Domain.TaskStatus.Cancelled);
        var task = dbContext.Tasks.Single(t => t.Id == 2);

        Assert.Equal(Domain.TaskStatus.Cancelled, task.Status);
    }

    [Fact]
    public void ChangeStatus_CompletedTask_Throws()
    {
        var taskService = CreateTaskService(out var dbContext);
        var exception = Assert.Throws<InvalidOperationException>(() => taskService.ChangeStatus(3, Domain.TaskStatus.InProgress));

        Assert.Equal("Invalid status transition.", exception.Message);
    }

    [Fact]
    public void ChangeStatus_CancelledTask_Throws()
    {
        var taskService = CreateTaskService(out var dbContext);
        taskService.ChangeStatus(1, Domain.TaskStatus.Cancelled);
        var exception = Assert.Throws<InvalidOperationException>(() => taskService.ChangeStatus(1, Domain.TaskStatus.InProgress));

        Assert.Equal("Invalid status transition.", exception.Message);
    }

    [Fact]
    public void GetTasksByAssignee_ReturnsTasksForSpecifiedAssignee()
    {
        var taskService = CreateTaskService(out _);

        var result = taskService.GetTasksByAssignee(2);

        Assert.NotEmpty(result);
        Assert.All(result, task => Assert.Equal(2, task.AssigneeId));
    }

    [Fact]
    public void GetTasksByAssignee_ReturnsAllTasksForSpecifiedAssignee()
    {
        var taskService = CreateTaskService(out var dbContext);

        var existingTasksCount = dbContext.Tasks.Count(t => t.AssigneeId == 2);

        dbContext.Tasks.Add(new TaskItem
        {
            Title = "Additional task",
            Description = "Test description",
            PlannedStartAt = PlannedStartAt,
            DueAt = DueAt,
            Status = Domain.TaskStatus.New,
            CreatedByEmployeeId = 1,
            AssigneeId = 2
        });

        dbContext.SaveChanges();

        var result = taskService.GetTasksByAssignee(2);

        Assert.Equal(existingTasksCount + 1, result.Count);
        Assert.All(result, task => Assert.Equal(2, task.AssigneeId));
    }

    [Fact]
    public void GetTasksByAssignee_NoTasks_ReturnsEmptyList()
    {
        var taskService = CreateTaskService(out _);
        var result = taskService.GetTasksByAssignee(3);

        Assert.Empty(result);
    }

}