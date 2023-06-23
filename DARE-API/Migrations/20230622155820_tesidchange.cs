using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DARE_API.Migrations
{
    /// <inheritdoc />
    public partial class tesidchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Endpoints_EndpointsId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_EndpointsId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EndpointsId",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "TesId",
                table: "Submissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "EndpointsProjects",
                columns: table => new
                {
                    EndpointsId = table.Column<int>(type: "integer", nullable: false),
                    ProjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointsProjects", x => new { x.EndpointsId, x.ProjectsId });
                    table.ForeignKey(
                        name: "FK_EndpointsProjects_Endpoints_EndpointsId",
                        column: x => x.EndpointsId,
                        principalTable: "Endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EndpointsProjects_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EndpointsProjects_ProjectsId",
                table: "EndpointsProjects",
                column: "ProjectsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EndpointsProjects");

            migrationBuilder.AlterColumn<int>(
                name: "TesId",
                table: "Submissions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "EndpointsId",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EndpointsId",
                table: "Projects",
                column: "EndpointsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Endpoints_EndpointsId",
                table: "Projects",
                column: "EndpointsId",
                principalTable: "Endpoints",
                principalColumn: "Id");
        }
    }
}
