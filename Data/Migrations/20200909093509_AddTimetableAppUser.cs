using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class AddTimetableAppUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndLunchTime",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndWorkTime",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartLunchTime",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartWorkTime",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndLunchTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EndWorkTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StartLunchTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StartWorkTime",
                table: "AspNetUsers");
        }
    }
}
