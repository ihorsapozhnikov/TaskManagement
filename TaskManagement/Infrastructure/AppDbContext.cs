using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain;
using TaskManagement.Infrastructure.Seed;

namespace TaskManagement.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData.Configure(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}