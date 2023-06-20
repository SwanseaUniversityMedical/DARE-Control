using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BL.Migrations
{
    /// <inheritdoc />
    public partial class endpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndpointsId",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Endpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FormIoUrl = table.Column<string>(type: "text", nullable: false),
                    Json = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    FormIoString = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EndpointsId",
                table: "Projects",
                column: "EndpointsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Endpoints_EndpointsId",
                table: "Projects",
                column: "EndpointsId",
                principalTable: "Endpoints",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Endpoints_EndpointsId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Endpoints");

            migrationBuilder.DropTable(
                name: "FormData");

            migrationBuilder.DropIndex(
                name: "IX_Projects_EndpointsId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EndpointsId",
                table: "Projects");
        }
    }
}
