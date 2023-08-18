using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DARE_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAboutColumntoEndpointTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "Endpoints",
                type: "text",
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "About",
                table: "Endpoints");
        }
    }
}
