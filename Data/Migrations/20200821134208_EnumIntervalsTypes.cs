using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class EnumIntervalsTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "IntervalRecords");

            migrationBuilder.AddColumn<int>(
                name: "IntervalType",
                table: "IntervalRecords",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalType",
                table: "IntervalRecords");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "IntervalRecords",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
