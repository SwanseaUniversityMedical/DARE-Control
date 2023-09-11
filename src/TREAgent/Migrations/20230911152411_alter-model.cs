using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TREAgent.Migrations
{
    /// <inheritdoc />
    public partial class altermodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "teskid",
                schema: "agent",
                table: "TESK_Audit",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "teskid",
                schema: "agent",
                table: "TESK_Audit");
        }
    }
}
