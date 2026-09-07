using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailNotificationsToSupportPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailNotificationsEnabled",
                table: "SupportPlans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailNotificationsEnabled",
                table: "SupportPlans");
        }
    }
}
