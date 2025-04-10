using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Licenta_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class Create_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Requests_RequestId",
                table: "Files");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Files_RequestId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "DateUploaded",
                table: "Files",
                newName: "UploadedDate");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RegistrationRequestId",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RegistrationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    RegistrationSessionId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProposedTheme = table.Column<string>(type: "text", nullable: false),
                    StatusJustification = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationRequests_RegistrationSessions_RegistrationSessi~",
                        column: x => x.RegistrationSessionId,
                        principalTable: "RegistrationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationRequests_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_RegistrationRequestId",
                table: "Files",
                column: "RegistrationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_RegistrationSessionId",
                table: "RegistrationRequests",
                column: "RegistrationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_StudentId",
                table: "RegistrationRequests",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_RegistrationRequests_RegistrationRequestId",
                table: "Files",
                column: "RegistrationRequestId",
                principalTable: "RegistrationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_RegistrationRequests_RegistrationRequestId",
                table: "Files");

            migrationBuilder.DropTable(
                name: "RegistrationRequests");

            migrationBuilder.DropIndex(
                name: "IX_Files_RegistrationRequestId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "RegistrationRequestId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "UploadedDate",
                table: "Files",
                newName: "DateUploaded");

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfessorId = table.Column<int>(type: "integer", nullable: false),
                    RegistrationSessionId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Professors_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Professors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requests_RegistrationSessions_RegistrationSessionId",
                        column: x => x.RegistrationSessionId,
                        principalTable: "RegistrationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requests_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_RequestId",
                table: "Files",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ProfessorId",
                table: "Requests",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RegistrationSessionId",
                table: "Requests",
                column: "RegistrationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_StudentId",
                table: "Requests",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Requests_RequestId",
                table: "Files",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
