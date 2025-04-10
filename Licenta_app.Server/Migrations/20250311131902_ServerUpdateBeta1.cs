using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Licenta_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class ServerUpdateBeta1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Professors_HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Professors",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "HeadOfDepartmentId",
                table: "Departments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Professors_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId",
                principalTable: "Professors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Professors_HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Professors",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HeadOfDepartmentId",
                table: "Departments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Professors_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId",
                principalTable: "Professors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
