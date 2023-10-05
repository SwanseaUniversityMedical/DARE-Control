using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRE_API.Migrations
{
    /// <inheritdoc />
    public partial class passwordfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Projects");
        }
    }
}
