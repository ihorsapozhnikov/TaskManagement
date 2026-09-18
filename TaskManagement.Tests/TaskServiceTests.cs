using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain;
using TaskManagement.Infrastructure;
using TaskManagement.Services;
using Microsoft.Extensions.Time.Testing;

namespace TaskManagement.Tests;

public class TaskServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly TaskService _taskService;

    private readonly FakeTimeProvider _timeProvider;

    private static readonly DateTime PlannedStartAt = new(2026, 9, 17, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime DueAt = new(2026, 9, 17, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime InvalidPlannedStartAt = new(2026, 9, 17, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime InvalidDueAt = new(2026, 9, 17, 10, 0, 0, DateTimeKind.Utc);

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 9, 17, 15, 0, 0, TimeSpan.Zero));

        _taskService = new TaskService(_dbContext, _timeProvider);
    }

    [Fact]
    public void CreateTask_InactiveAssignee_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _taskService.CreateTask(
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
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _taskService.CreateTask(
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
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _taskService.CreateTask(
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
        var task = _taskService.CreateTask(
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

        var savedTask = _dbContext.Tasks.Single(t => t.Id == task.Id);

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
        _taskService.ChangeStatus(1, Domain.TaskStatus.InProgress);
        var task = _dbContext.Tasks.Single(t => t.Id == 1);

        Assert.Equal(Domain.TaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void ChangeStatus_NewToCancelled_ChangesStatus()
    {
        _taskService.ChangeStatus(1, Domain.TaskStatus.Cancelled);
        var task = _dbContext.Tasks.Single(t => t.Id == 1);

        Assert.Equal(Domain.TaskStatus.Cancelled, task.Status);
    }

    [Fact]
    public void ChangeStatus_InProgressToCompleted_SetsCompletedAt()
    {
        var task = _dbContext.Tasks.Single(t => t.Id == 2);

        var expectedCompletedAt = new DateTime(2026, 9, 17, 15, 0, 0, DateTimeKind.Utc);

        _timeProvider.SetUtcNow(expectedCompletedAt);

        _taskService.ChangeStatus(task.Id, Domain.TaskStatus.Completed);

        Assert.Equal(Domain.TaskStatus.Completed, task.Status);
        Assert.Equal(expectedCompletedAt, task.CompletedAt);
    }

    [Fact]
    public void ChangeStatus_InProgressToCancelled_ChangesStatus()
    {
        _taskService.ChangeStatus(2, Domain.TaskStatus.Cancelled);
        var task = _dbContext.Tasks.Single(t => t.Id == 2);

        Assert.Equal(Domain.TaskStatus.Cancelled, task.Status);
    }

    [Fact]
    public void ChangeStatus_CompletedTask_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => _taskService.ChangeStatus(3, Domain.TaskStatus.InProgress));

        Assert.Equal("Invalid status transition.", exception.Message);
    }

    [Fact]
    public void ChangeStatus_CancelledTask_Throws()
    {
        _taskService.ChangeStatus(1, Domain.TaskStatus.Cancelled);
        var exception = Assert.Throws<InvalidOperationException>(() => _taskService.ChangeStatus(1, Domain.TaskStatus.InProgress));

        Assert.Equal("Invalid status transition.", exception.Message);
    }

    [Fact]
    public void GetTasksByAssignee_ReturnsTasksForSpecifiedAssignee()
    {
        var result = _taskService.GetTasksByAssignee(2);

        Assert.NotEmpty(result);
        Assert.All(result, task => Assert.Equal(2, task.AssigneeId));
    }

    [Fact]
    public void GetTasksByAssignee_ReturnsAllTasksForSpecifiedAssignee()
    {
        var existingTasksCount = _dbContext.Tasks.Count(t => t.AssigneeId == 2);

        _dbContext.Tasks.Add(new TaskItem
        {
            Title = "Additional task",
            Description = "Test description",
            PlannedStartAt = PlannedStartAt,
            DueAt = DueAt,
            Status = Domain.TaskStatus.New,
            CreatedByEmployeeId = 1,
            AssigneeId = 2
        });

        _dbContext.SaveChanges();

        var result = _taskService.GetTasksByAssignee(2);

        Assert.Equal(existingTasksCount + 1, result.Count);
        Assert.All(result, task => Assert.Equal(2, task.AssigneeId));
    }

    [Fact]
    public void GetTasksByAssignee_NoTasks_ReturnsEmptyList()
    {
        var result = _taskService.GetTasksByAssignee(3);

        Assert.Empty(result);
    }

    [Fact]
    public void CreateTask_AssigneeNotFound_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _taskService.CreateTask(
                title: "Test task",
                description: "Test description",
                plannedStartAt: PlannedStartAt,
                dueAt: DueAt,
                createdByEmployeeId: 1,
                assigneeId: int.MaxValue));

        Assert.Equal("Assignee not found.", exception.Message);
    }

    [Fact]
    public void CreateTask_CreatorNotFound_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _taskService.CreateTask(
                title: "Test task",
                description: "Test description",
                plannedStartAt: PlannedStartAt,
                dueAt: DueAt,
                createdByEmployeeId: int.MaxValue,
                assigneeId: 2));

        Assert.Equal("Creator not found.", exception.Message);
    }

    [Fact]
    public void ChangeStatus_TaskNotFound_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => _taskService.ChangeStatus(int.MaxValue, Domain.TaskStatus.InProgress));

        Assert.Equal("Task not found.", exception.Message);
    }

    [Fact]
    public void CreateTask_NonUtcTimestamps_Throws()
    {
        var plannedStartAt = new DateTime(2026, 9, 17, 10, 0, 0);
        var dueAt = new DateTime(2026, 9, 17, 18, 0, 0);

        var exception = Assert.Throws<ArgumentException>(() =>
            _taskService.CreateTask(
                title: "Test task",
                description: "Test description",
                plannedStartAt: plannedStartAt,
                dueAt: dueAt,
                createdByEmployeeId: 1,
                assigneeId: 2));

        Assert.Equal("All timestamps must be in UTC.", exception.Message);
    }
}