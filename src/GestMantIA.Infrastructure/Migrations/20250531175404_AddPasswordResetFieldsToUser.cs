using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestMantIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                schema: "identity",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiration",
                schema: "identity",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiration",
                schema: "identity",
                table: "Users");
        }
    }
}
