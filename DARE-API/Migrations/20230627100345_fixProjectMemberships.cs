using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DARE_API.Migrations
{
    /// <inheritdoc />
    public partial class fixProjectMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Projects_ProjectsId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProjectsId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProjectsId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "ProjectsUser",
                columns: table => new
                {
                    ProjectsId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectsUser", x => new { x.ProjectsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ProjectsUser_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectsUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectsUser_UsersId",
                table: "ProjectsUser",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectsUser");

            migrationBuilder.AddColumn<int>(
                name: "ProjectsId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProjectsId",
                table: "Users",
                column: "ProjectsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Projects_ProjectsId",
                table: "Users",
                column: "ProjectsId",
                principalTable: "Projects",
                principalColumn: "Id");
        }
    }
}
