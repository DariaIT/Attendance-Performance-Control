using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class DayTimeIntervalDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DayRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntervalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DayRecordId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntervalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntervalRecords_DayRecords_DayRecordId",
                        column: x => x.DayRecordId,
                        principalTable: "DayRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: true),
                    RecordTime = table.Column<DateTime>(nullable: false),
                    DayRecordId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeRecords_DayRecords_DayRecordId",
                        column: x => x.DayRecordId,
                        principalTable: "DayRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DayRecords_UserId",
                table: "DayRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IntervalRecords_DayRecordId",
                table: "IntervalRecords",
                column: "DayRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecords_DayRecordId",
                table: "TimeRecords",
                column: "DayRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntervalRecords");

            migrationBuilder.DropTable(
                name: "TimeRecords");

            migrationBuilder.DropTable(
                name: "DayRecords");
        }
    }
}
