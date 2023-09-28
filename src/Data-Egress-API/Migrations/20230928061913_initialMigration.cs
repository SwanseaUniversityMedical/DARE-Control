using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data_Egress_API.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataEgressFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    submissionId = table.Column<int>(type: "integer", nullable: false),
                    FileSize = table.Column<string>(type: "text", nullable: true),
                    FileStatus = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    FileType = table.Column<string>(type: "text", nullable: true),
                    FileData = table.Column<byte[]>(type: "bytea", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reviewer = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataEgressFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    File = table.Column<byte[]>(type: "bytea", nullable: true),
                    DataEgressFilesId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_DataEgressFile_DataEgressFilesId",
                        column: x => x.DataEgressFilesId,
                        principalTable: "DataEgressFile",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_DataEgressFilesId",
                table: "Files",
                column: "DataEgressFilesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "DataEgressFile");
        }
    }
}
