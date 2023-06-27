using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DARE_API.Migrations
{
    /// <inheritdoc />
    public partial class endpointformdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Endpoints_Id",
                table: "FormData",
                column: "Id",
                principalTable: "Endpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Endpoints_Id",
                table: "FormData");
        }
    }
}
