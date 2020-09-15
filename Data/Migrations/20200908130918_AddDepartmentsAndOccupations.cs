using Microsoft.EntityFrameworkCore.Migrations;

namespace Attendance_Performance_Control.Data.Migrations
{
    public partial class AddDepartmentsAndOccupations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "OccupationId",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Occupations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcupationName = table.Column<string>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Occupations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Occupations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "DepartmentName" },
                values: new object[,]
                {
                    { 1, "Dep. SO" },
                    { 2, "Dep. Formação" },
                    { 3, "Dep. Admin" },
                    { 4, "Dep. Comercial" },
                    { 5, "Dep. HST" }
                });

            migrationBuilder.InsertData(
                table: "Occupations",
                columns: new[] { "Id", "DepartmentId", "OcupationName" },
                values: new object[,]
                {
                    { 1, 1, "Coordenadora Dep. Saúde" },
                    { 2, 1, "Gestor Clientes Saúde" },
                    { 3, 1, "TDT (Técnico de Diagnóstico e Terapêutica)" },
                    { 4, 1, "Enfermeira" },
                    { 5, 2, "Gestor de Formação" },
                    { 6, 3, "Diretor Geral" },
                    { 7, 3, "Assessora Financeira" },
                    { 8, 3, "Técnico Administrativo" },
                    { 9, 3, "Empregada de Limpeza" },
                    { 10, 4, "Técnico Comercial" },
                    { 11, 5, "TSST" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OccupationId",
                table: "AspNetUsers",
                column: "OccupationId");

            migrationBuilder.CreateIndex(
                name: "IX_Occupations_DepartmentId",
                table: "Occupations",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Occupations_OccupationId",
                table: "AspNetUsers",
                column: "OccupationId",
                principalTable: "Occupations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Occupations_OccupationId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Occupations");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OccupationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OccupationId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "Occupation",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
