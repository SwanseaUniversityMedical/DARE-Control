using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRE_API.Migrations
{
    /// <inheritdoc />
    public partial class TreMembershipDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipDecisions_Projects_ProjectId",
                table: "MembershipDecisions");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipDecisions_Users_UserId",
                table: "MembershipDecisions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembershipDecisions",
                table: "MembershipDecisions");

            migrationBuilder.RenameTable(
                name: "MembershipDecisions",
                newName: "TreMembershipDecision");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipDecisions_UserId",
                table: "TreMembershipDecision",
                newName: "IX_TreMembershipDecision_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipDecisions_ProjectId",
                table: "TreMembershipDecision",
                newName: "IX_TreMembershipDecision_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TreMembershipDecision",
                table: "TreMembershipDecision",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TreMembershipDecision_Projects_ProjectId",
                table: "TreMembershipDecision",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TreMembershipDecision_Users_UserId",
                table: "TreMembershipDecision",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreMembershipDecision_Projects_ProjectId",
                table: "TreMembershipDecision");

            migrationBuilder.DropForeignKey(
                name: "FK_TreMembershipDecision_Users_UserId",
                table: "TreMembershipDecision");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TreMembershipDecision",
                table: "TreMembershipDecision");

            migrationBuilder.RenameTable(
                name: "TreMembershipDecision",
                newName: "MembershipDecisions");

            migrationBuilder.RenameIndex(
                name: "IX_TreMembershipDecision_UserId",
                table: "MembershipDecisions",
                newName: "IX_MembershipDecisions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TreMembershipDecision_ProjectId",
                table: "MembershipDecisions",
                newName: "IX_MembershipDecisions_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembershipDecisions",
                table: "MembershipDecisions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipDecisions_Projects_ProjectId",
                table: "MembershipDecisions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipDecisions_Users_UserId",
                table: "MembershipDecisions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
