using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licenta_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class Create_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Professors_Departments_DepartmentId",
                table: "Professors");

            migrationBuilder.DropIndex(
                name: "IX_Professors_DepartmentId",
                table: "Professors");

            migrationBuilder.AddColumn<int>(
                name: "HeadOfDepartmentId",
                table: "Departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Professors_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId",
                principalTable: "Professors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Professors_HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.CreateIndex(
                name: "IX_Professors_DepartmentId",
                table: "Professors",
                column: "DepartmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Professors_Departments_DepartmentId",
                table: "Professors",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
