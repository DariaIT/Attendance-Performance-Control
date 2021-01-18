using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class AddUserAndBankHolidays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankHolidaysType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankHolidayTypeName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankHolidaysType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserHolidays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayDay = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHolidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHolidays_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankHolidays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(nullable: false),
                    BankHolidaysTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankHolidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankHolidays_BankHolidaysType_BankHolidaysTypeId",
                        column: x => x.BankHolidaysTypeId,
                        principalTable: "BankHolidaysType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BankHolidaysType",
                columns: new[] { "Id", "BankHolidayTypeName" },
                values: new object[,]
                {
                    { 1, "Feriado" },
                    { 2, "Tolerância de Ponto" },
                    { 3, "Confinamento Obrigatório" },
                    { 4, "Confinamento Facultativo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankHolidays_BankHolidaysTypeId",
                table: "BankHolidays",
                column: "BankHolidaysTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHolidays_UserId",
                table: "UserHolidays",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankHolidays");

            migrationBuilder.DropTable(
                name: "UserHolidays");

            migrationBuilder.DropTable(
                name: "BankHolidaysType");
        }
    }
}
