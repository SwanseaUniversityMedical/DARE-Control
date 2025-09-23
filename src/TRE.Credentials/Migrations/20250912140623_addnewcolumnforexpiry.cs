using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tre_Credentials.Migrations
{
    /// <inheritdoc />
    public partial class addnewcolumnforexpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "EphemeralCredentials",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "EphemeralCredentials");
        }
    }
}
