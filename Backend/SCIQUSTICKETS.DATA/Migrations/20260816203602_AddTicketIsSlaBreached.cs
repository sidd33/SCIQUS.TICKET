using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketIsSlaBreached : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSlaBreached",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSlaBreached",
                table: "Tickets");
        }
    }
}
