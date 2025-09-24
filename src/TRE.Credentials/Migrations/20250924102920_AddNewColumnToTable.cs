using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tre_Credentials.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CredentialType",
                table: "EphemeralCredentials",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CredentialType",
                table: "EphemeralCredentials");
        }
    }
}
