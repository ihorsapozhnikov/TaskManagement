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

    private static TaskService CreateTaskService(out AppDbContext dbContext)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        dbContext = new AppDbContext(options);

        dbContext.Database.EnsureCreated();

        return new TaskService(dbContext);
    }
}