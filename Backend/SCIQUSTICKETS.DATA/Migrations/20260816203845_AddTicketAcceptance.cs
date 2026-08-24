using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketAcceptance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptanceDeadlineAt",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcceptanceDeadlineHours",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptanceStatus",
                table: "Tickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "AcceptanceDeadlineAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AcceptanceDeadlineHours",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AcceptanceStatus",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "DefaultAcceptanceDeadlineHours",
                table: "SlaConfigurations");

            migrationBuilder.DropColumn(
                name: "RequiresAcceptanceGlobalDefault",
                table: "SlaConfigurations");
        }
    }
}
