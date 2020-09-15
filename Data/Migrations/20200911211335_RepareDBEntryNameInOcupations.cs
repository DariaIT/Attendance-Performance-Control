using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class RepareDBEntryNameInOcupations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OcupationName",
                table: "Occupations");

            migrationBuilder.AddColumn<string>(
                name: "OccupationName",
                table: "Occupations",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 1,
                column: "OccupationName",
                value: "Coordenadora Dep. Saúde");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 2,
                column: "OccupationName",
                value: "Gestor Clientes Saúde");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 3,
                column: "OccupationName",
                value: "TDT (Técnico de Diagnóstico e Terapêutica)");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 4,
                column: "OccupationName",
                value: "Enfermeira");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 5,
                column: "OccupationName",
                value: "Gestor de Formação");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 6,
                column: "OccupationName",
                value: "Diretor Geral");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 7,
                column: "OccupationName",
                value: "Assessora Financeira");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 8,
                column: "OccupationName",
                value: "Técnico Administrativo");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 9,
                column: "OccupationName",
                value: "Empregada de Limpeza");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 10,
                column: "OccupationName",
                value: "Técnico Comercial");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 11,
                column: "OccupationName",
                value: "TSST");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccupationName",
                table: "Occupations");

            migrationBuilder.AddColumn<string>(
                name: "OcupationName",
                table: "Occupations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 1,
                column: "OcupationName",
                value: "Coordenadora Dep. Saúde");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 2,
                column: "OcupationName",
                value: "Gestor Clientes Saúde");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 3,
                column: "OcupationName",
                value: "TDT (Técnico de Diagnóstico e Terapêutica)");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 4,
                column: "OcupationName",
                value: "Enfermeira");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 5,
                column: "OcupationName",
                value: "Gestor de Formação");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 6,
                column: "OcupationName",
                value: "Diretor Geral");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 7,
                column: "OcupationName",
                value: "Assessora Financeira");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 8,
                column: "OcupationName",
                value: "Técnico Administrativo");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 9,
                column: "OcupationName",
                value: "Empregada de Limpeza");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 10,
                column: "OcupationName",
                value: "Técnico Comercial");

            migrationBuilder.UpdateData(
                table: "Occupations",
                keyColumn: "Id",
                keyValue: 11,
                column: "OcupationName",
                value: "TSST");
        }
    }
}
