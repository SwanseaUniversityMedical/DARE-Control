using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRE_API.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Projectname",
                table: "ProjectApproval",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "ProjectApproval",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Projectname",
                table: "ProjectApproval");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "ProjectApproval");
        }
    }
}
