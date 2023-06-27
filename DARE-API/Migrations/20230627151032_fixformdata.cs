using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DARE_API.Migrations
{
    /// <inheritdoc />
    public partial class fixformdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Endpoints_Id",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Projects_Id",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Users_Id",
                table: "FormData");

            migrationBuilder.AddColumn<string>(
                name: "FormData",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FormData",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FormData",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "FormData",
                table: "Endpoints",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormData",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FormData",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "FormData",
                table: "Endpoints");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FormData",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Endpoints_Id",
                table: "FormData",
                column: "Id",
                principalTable: "Endpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Projects_Id",
                table: "FormData",
                column: "Id",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Users_Id",
                table: "FormData",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
