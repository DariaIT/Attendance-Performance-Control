using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class AddIntervalTypeAsStringToIntervalRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IntervalType",
                table: "IntervalRecords",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IntervalType",
                table: "IntervalRecords",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
