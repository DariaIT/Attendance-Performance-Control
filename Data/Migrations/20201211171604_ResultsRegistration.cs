using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class ResultsRegistration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResultTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResultTypeName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(nullable: false),
                    ResultTypeId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_ResultTypes_ResultTypeId",
                        column: x => x.ResultTypeId,
                        principalTable: "ResultTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ResultTypes",
                columns: new[] { "Id", "ResultTypeName" },
                values: new object[] { 1, "Auditoria" });

            migrationBuilder.InsertData(
                table: "ResultTypes",
                columns: new[] { "Id", "ResultTypeName" },
                values: new object[] { 2, "Consulta" });

            migrationBuilder.InsertData(
                table: "ResultTypes",
                columns: new[] { "Id", "ResultTypeName" },
                values: new object[] { 3, "Relatório" });

            migrationBuilder.CreateIndex(
                name: "IX_Results_ResultTypeId",
                table: "Results",
                column: "ResultTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_UserId",
                table: "Results",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "ResultTypes");
        }
    }
}
