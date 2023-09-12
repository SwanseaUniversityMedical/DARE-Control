using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRE_API.Migrations
{
    /// <inheritdoc />
    public partial class minioBucketAddedForTreProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OutputBucketTre",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubmissionBucketTre",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputBucketTre",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SubmissionBucketTre",
                table: "Projects");
        }
    }
}
