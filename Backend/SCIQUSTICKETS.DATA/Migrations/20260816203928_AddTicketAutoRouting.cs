using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketAutoRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxConcurrentOpenTickets",
                table: "Departments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxConsecutiveAssignments",
                table: "Departments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecencyCapHours",
                table: "Departments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketAutoAssignMethod",
                table: "Departments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "W_Load",
                table: "Departments",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "W_Recency",
                table: "Departments",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "W_Severity",
                table: "Departments",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxConcurrentOpenTickets",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "MaxConsecutiveAssignments",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "RecencyCapHours",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "TicketAutoAssignMethod",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "W_Load",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "W_Recency",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "W_Severity",
                table: "Departments");
        }
    }
}
