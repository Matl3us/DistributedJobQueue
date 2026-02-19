using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobQueue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessages",
                table: "Jobs",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryAt",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<short>(
                name: "RetryCount",
                table: "Jobs",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessages",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Jobs");
        }
    }
}
