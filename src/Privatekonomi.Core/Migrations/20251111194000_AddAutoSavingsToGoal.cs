using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Privatekonomi.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoSavingsToGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutoSavingsType",
                table: "Goals",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyAutoSavingsAmount",
                table: "Goals",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoSavingsType",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "MonthlyAutoSavingsAmount",
                table: "Goals");
        }
    }
}
