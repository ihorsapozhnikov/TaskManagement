# TaskManagement

TaskManagement is a backend/data layer for an internal CRM Task Management module.
The project provides task creation, assignment, status management, validation of business rules, and persistence using Entity Framework Core and PostgreSQL.

## Project structure

* `TaskManagement` — main project (domain, infrastructure, services).

  * `Domain` — domain entities and enums (`Employee`, `TaskItem`, `TaskStatus`).
  * `Infrastructure` — EF Core `AppDbContext`, Fluent API configurations, migrations, and seed data.
  * `Services` — business logic (`TaskService`) enforcing task creation and status transition rules.
* `TaskManagement.Tests` — automated tests for service behavior and business rules.

## Key features

* Create and assign tasks with planned start and due dates.
* Task status transitions (`New` → `InProgress`, `New` → `Cancelled`, `InProgress` → `Completed`, `InProgress` → `Cancelled`).
* `Completed` and `Cancelled` are final statuses.
* Validation rules:

  * assignee must be active;
  * creator and assignee must be different;
  * creator and assignee must exist;
  * `DueAt` cannot be earlier than `PlannedStartAt`;
  * `CompletedAt` is required only for `Completed` status.
* PostgreSQL check constraints for critical business rules.
* Index on `AssigneeId` for retrieving tasks by assignee.
* Seed data for employees and tasks.
* EF Core migrations.
* In-memory database provider used for tests.

## Technologies used

* .NET 10
* Entity Framework Core 10
* PostgreSQL
* xUnit
* EF Core InMemory provider for tests
* Microsoft.Extensions.TimeProvider.Testing

## Time Contract

All application timestamps use UTC.

### Rules

* All `DateTime` values accepted by the service layer must have `DateTimeKind.Utc`.
* Local and `DateTimeKind.Unspecified` values are not accepted.
* Timestamps represent an absolute point in time and are handled by the application as UTC.
* `CompletedAt` is assigned using the injected `TimeProvider` when a task transitions to `Completed`.
* The production implementation uses the system UTC time.
* Tests use a fake time provider to make time-dependent behavior deterministic.
* `CompletedAt` is `null` for tasks that are not in the `Completed` state.
* The service validates timestamp values before performing database operations.

## How to run

* Restore dependencies: `dotnet restore`
* Build the solution: `dotnet build`
* Apply EF Core migrations: `dotnet ef database update`
* Run tests: `dotnet test`

All commands should be executed from the solution root.

## Notes

* Business logic is implemented in `TaskService` and covered by automated tests in `TaskManagement.Tests`.
* Entity configuration is implemented using EF Core Fluent API without data annotations.
* PostgreSQL is used as the production database provider.
* Tests use EF Core InMemory with a separate database instance for each test.
