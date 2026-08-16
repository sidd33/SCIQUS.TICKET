using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddAcceptanceGlobalDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultAcceptanceDeadlineHours",
                table: "SlaConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAcceptanceGlobalDefault",
                table: "SlaConfigurations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultAcceptanceDeadlineHours",
                table: "SlaConfigurations");

            migrationBuilder.DropColumn(
                name: "RequiresAcceptanceGlobalDefault",
                table: "SlaConfigurations");
        }
    }
}
