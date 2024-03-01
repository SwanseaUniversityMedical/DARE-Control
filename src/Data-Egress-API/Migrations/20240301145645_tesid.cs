using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Egress_API.Migrations
{
    /// <inheritdoc />
    public partial class tesid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubFolder",
                table: "EgressSubmissions",
                newName: "tesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "tesId",
                table: "EgressSubmissions",
                newName: "SubFolder");
        }
    }
}
