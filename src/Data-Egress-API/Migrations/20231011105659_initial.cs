using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data_Egress_API.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EgressSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OutputBucket = table.Column<string>(type: "text", nullable: true),
                    Completed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reviewer = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EgressSubmissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeycloakCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    PasswordEnc = table.Column<string>(type: "text", nullable: false),
                    CredentialType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeycloakCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EgressFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reviewer = table.Column<string>(type: "text", nullable: true),
                    EgressSubmissionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EgressFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EgressFiles_EgressSubmissions_EgressSubmissionId",
                        column: x => x.EgressSubmissionId,
                        principalTable: "EgressSubmissions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EgressFiles_EgressSubmissionId",
                table: "EgressFiles",
                column: "EgressSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EgressFiles");

            migrationBuilder.DropTable(
                name: "KeycloakCredentials");

            migrationBuilder.DropTable(
                name: "EgressSubmissions");
        }
    }
}
