using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TRE_API.Migrations
{
    /// <inheritdoc />
    public partial class ProjectTreDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormData = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Display = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectDescription = table.Column<string>(type: "text", nullable: false),
                    MarkAsEmbargoed = table.Column<bool>(type: "boolean", nullable: false),
                    SubmissionBucket = table.Column<string>(type: "text", nullable: true),
                    OutputBucket = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LastHeartBeatReceived = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdminUsername = table.Column<string>(type: "text", nullable: false),
                    About = table.Column<string>(type: "text", nullable: false),
                    FormData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FormData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTre",
                columns: table => new
                {
                    ProjectsId = table.Column<int>(type: "integer", nullable: false),
                    TresId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTre", x => new { x.ProjectsId, x.TresId });
                    table.ForeignKey(
                        name: "FK_ProjectTre_Project_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTre_Tre_TresId",
                        column: x => x.TresId,
                        principalTable: "Tre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTreDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionProjId = table.Column<int>(type: "integer", nullable: true),
                    TreId = table.Column<int>(type: "integer", nullable: true),
                    Decision = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTreDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTreDecisions_Project_SubmissionProjId",
                        column: x => x.SubmissionProjId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectTreDecisions_Tre_TreId",
                        column: x => x.TreId,
                        principalTable: "Tre",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectUser",
                columns: table => new
                {
                    ProjectsId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectUser", x => new { x.ProjectsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ProjectUser_Project_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectUser_User_UsersId",
                        column: x => x.UsersId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Submission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    TesId = table.Column<string>(type: "text", nullable: true),
                    SourceCrate = table.Column<string>(type: "text", nullable: false),
                    TesName = table.Column<string>(type: "text", nullable: false),
                    TesJson = table.Column<string>(type: "text", nullable: true),
                    DockerInputLocation = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    ParentID = table.Column<int>(type: "integer", nullable: true),
                    TreId = table.Column<int>(type: "integer", nullable: true),
                    SubmittedById = table.Column<int>(type: "integer", nullable: false),
                    LastStatusUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submission_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submission_Submission_ParentID",
                        column: x => x.ParentID,
                        principalTable: "Submission",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Submission_Tre_TreId",
                        column: x => x.TreId,
                        principalTable: "Tre",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Submission_User_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricStatus_Submission_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TreBucketFullPath = table.Column<string>(type: "text", nullable: false),
                    SubmisionBucketFullPath = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionFile_Submission_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricStatus_SubmissionId",
                table: "HistoricStatus",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTre_TresId",
                table: "ProjectTre",
                column: "TresId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTreDecisions_SubmissionProjId",
                table: "ProjectTreDecisions",
                column: "SubmissionProjId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTreDecisions_TreId",
                table: "ProjectTreDecisions",
                column: "TreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUser_UsersId",
                table: "ProjectUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_ParentID",
                table: "Submission",
                column: "ParentID");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_ProjectId",
                table: "Submission",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_SubmittedById",
                table: "Submission",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_TreId",
                table: "Submission",
                column: "TreId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionFile_SubmissionId",
                table: "SubmissionFile",
                column: "SubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricStatus");

            migrationBuilder.DropTable(
                name: "ProjectTre");

            migrationBuilder.DropTable(
                name: "ProjectTreDecisions");

            migrationBuilder.DropTable(
                name: "ProjectUser");

            migrationBuilder.DropTable(
                name: "SubmissionFile");

            migrationBuilder.DropTable(
                name: "Submission");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "Tre");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
