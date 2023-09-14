using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TREAgent.Migrations
{
    /// <inheritdoc />
    public partial class sortPgdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "dated",
                schema: "agent",
                table: "TESK_Audit",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "dated",
                schema: "agent",
                table: "TESK_Audit",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
