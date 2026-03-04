using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobQueue.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityPropertyToJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Jobs");
        }
    }
}
