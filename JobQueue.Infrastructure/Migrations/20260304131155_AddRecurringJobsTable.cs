using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobQueue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringJobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecurringJobId",
                table: "Jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecurringJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", maxLength: 4096, nullable: true),
                    CronExpression = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RecurringJobId",
                table: "Jobs",
                column: "RecurringJobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_RecurringJobs_RecurringJobId",
                table: "Jobs",
                column: "RecurringJobId",
                principalTable: "RecurringJobs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_RecurringJobs_RecurringJobId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "RecurringJobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_RecurringJobId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RecurringJobId",
                table: "Jobs");
        }
    }
}
