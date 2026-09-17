using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PlannedStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByEmployeeId = table.Column<int>(type: "integer", nullable: false),
                    AssigneeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.CheckConstraint("CK_Tasks_CompletedAt_Consistent_With_Status", "(\"Status\" = 2 AND \"CompletedAt\" IS NOT NULL) OR (\"Status\" <> 2 AND \"CompletedAt\" IS NULL)");
                    table.CheckConstraint("CK_Tasks_Creator_Different_From_Assignee", "\"CreatedByEmployeeId\" <> \"AssigneeId\"");
                    table.CheckConstraint("CK_Tasks_DueAt_GreaterThanOrEqual_PlannedStartAt", "\"DueAt\" >= \"PlannedStartAt\"");
                    table.CheckConstraint("CK_Tasks_Status_Valid", "\"Status\" IN (0, 1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_Tasks_Employees_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Employees_CreatedByEmployeeId",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "FullName", "IsActive" },
                values: new object[,]
                {
                    { 1, "Tom Johnson", true },
                    { 2, "David Tyson", true },
                    { 3, "Mike Fischer", false }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssigneeId", "CompletedAt", "CreatedByEmployeeId", "Description", "DueAt", "PlannedStartAt", "Status", "Title" },
                values: new object[,]
                {
                    { 1, 2, null, 1, "Prepare the monthly sales report.", new DateTime(2026, 9, 20, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 15, 9, 0, 0, 0, DateTimeKind.Utc), 0, "Prepare monthly report" },
                    { 2, 1, null, 2, "Update internal project documentation.", new DateTime(2026, 9, 18, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 10, 9, 0, 0, 0, DateTimeKind.Utc), 1, "Update documentation" },
                    { 3, 2, new DateTime(2026, 9, 4, 15, 30, 0, 0, DateTimeKind.Utc), 1, "Prepare presentation for the team meeting.", new DateTime(2026, 9, 5, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 1, 9, 0, 0, 0, DateTimeKind.Utc), 2, "Prepare presentation" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssigneeId",
                table: "Tasks",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedByEmployeeId",
                table: "Tasks",
                column: "CreatedByEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
