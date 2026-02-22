using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Privatekonomi.Core.Migrations;

/// <inheritdoc />
public partial class AddRecurringTasksAndKanbanStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Status",
            table: "HouseholdTasks",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IsRecurring",
            table: "HouseholdTasks",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "RecurrencePattern",
            table: "HouseholdTasks",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "RecurrenceInterval",
            table: "HouseholdTasks",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "NextDueDate",
            table: "HouseholdTasks",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ParentTaskId",
            table: "HouseholdTasks",
            type: "INTEGER",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Status",
            table: "HouseholdTasks");

        migrationBuilder.DropColumn(
            name: "IsRecurring",
            table: "HouseholdTasks");

        migrationBuilder.DropColumn(
            name: "RecurrencePattern",
            table: "HouseholdTasks");

        migrationBuilder.DropColumn(
            name: "RecurrenceInterval",
            table: "HouseholdTasks");

        migrationBuilder.DropColumn(
            name: "NextDueDate",
            table: "HouseholdTasks");

        migrationBuilder.DropColumn(
            name: "ParentTaskId",
            table: "HouseholdTasks");
    }
}
