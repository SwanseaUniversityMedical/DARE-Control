using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Egress_API.Migrations
{
    /// <inheritdoc />
    public partial class ProjectName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubFolder",
                table: "EgressSubmissions");

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "EgressSubmissions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "EgressSubmissions");

            migrationBuilder.AddColumn<string>(
                name: "SubFolder",
                table: "EgressSubmissions",
                type: "text",
                nullable: true);
        }
    }
}
