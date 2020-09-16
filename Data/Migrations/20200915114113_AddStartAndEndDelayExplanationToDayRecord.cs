using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class AddStartAndEndDelayExplanationToDayRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndDayDelayExplanation",
                table: "DayRecords",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartDayDelayExplanation",
                table: "DayRecords",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDayDelayExplanation",
                table: "DayRecords");

            migrationBuilder.DropColumn(
                name: "StartDayDelayExplanation",
                table: "DayRecords");
        }
    }
}
