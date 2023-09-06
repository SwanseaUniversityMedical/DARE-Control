using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DARE_API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsEmbargoedColumntoProjectsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmbargoed",
                table: "Projects",
                type: "boolean",
                nullable: true,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmbargoed",
                table: "Projects");
        }
    }
}
