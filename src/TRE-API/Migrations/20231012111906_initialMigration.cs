using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRE_API.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricStatus_Submission_SubmissionId",
                table: "HistoricStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_Project_ProjectId",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_Projects_TreProjectId",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_Submission_ParentID",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_Tre_TreId",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_Submission_User_SubmittedById",
                table: "Submission");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionFile_Submission_SubmissionId",
                table: "SubmissionFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submission",
                table: "Submission");

            migrationBuilder.RenameTable(
                name: "Submission",
                newName: "Submissions");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_TreProjectId",
                table: "Submissions",
                newName: "IX_Submissions_TreProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_TreId",
                table: "Submissions",
                newName: "IX_Submissions_TreId");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_SubmittedById",
                table: "Submissions",
                newName: "IX_Submissions_SubmittedById");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_ProjectId",
                table: "Submissions",
                newName: "IX_Submissions_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Submission_ParentID",
                table: "Submissions",
                newName: "IX_Submissions_ParentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submissions",
                table: "Submissions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricStatus_Submissions_SubmissionId",
                table: "HistoricStatus",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionFile_Submissions_SubmissionId",
                table: "SubmissionFile",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Project_ProjectId",
                table: "Submissions",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Projects_TreProjectId",
                table: "Submissions",
                column: "TreProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Submissions_ParentID",
                table: "Submissions",
                column: "ParentID",
                principalTable: "Submissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Tre_TreId",
                table: "Submissions",
                column: "TreId",
                principalTable: "Tre",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_User_SubmittedById",
                table: "Submissions",
                column: "SubmittedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricStatus_Submissions_SubmissionId",
                table: "HistoricStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionFile_Submissions_SubmissionId",
                table: "SubmissionFile");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Project_ProjectId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Projects_TreProjectId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Submissions_ParentID",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Tre_TreId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_User_SubmittedById",
                table: "Submissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Submissions",
                table: "Submissions");

            migrationBuilder.RenameTable(
                name: "Submissions",
                newName: "Submission");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_TreProjectId",
                table: "Submission",
                newName: "IX_Submission_TreProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_TreId",
                table: "Submission",
                newName: "IX_Submission_TreId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_SubmittedById",
                table: "Submission",
                newName: "IX_Submission_SubmittedById");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_ProjectId",
                table: "Submission",
                newName: "IX_Submission_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_ParentID",
                table: "Submission",
                newName: "IX_Submission_ParentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Submission",
                table: "Submission",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricStatus_Submission_SubmissionId",
                table: "HistoricStatus",
                column: "SubmissionId",
                principalTable: "Submission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_Project_ProjectId",
                table: "Submission",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_Projects_TreProjectId",
                table: "Submission",
                column: "TreProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_Submission_ParentID",
                table: "Submission",
                column: "ParentID",
                principalTable: "Submission",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_Tre_TreId",
                table: "Submission",
                column: "TreId",
                principalTable: "Tre",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Submission_User_SubmittedById",
                table: "Submission",
                column: "SubmittedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionFile_Submission_SubmissionId",
                table: "SubmissionFile",
                column: "SubmissionId",
                principalTable: "Submission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
