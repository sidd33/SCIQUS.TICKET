using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddSlaClosureAndReopenFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReopenedDate",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentTicketId",
                table: "Tickets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "ReopenCount",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ParentTicketId",
                table: "Tickets",
                column: "ParentTicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Tickets_ParentTicketId",
                table: "Tickets",
                column: "ParentTicketId",
                principalTable: "Tickets",
                principalColumn: "TicketId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Tickets_ParentTicketId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ParentTicketId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "LastReopenedDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ParentTicketId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReopenCount",
                table: "Tickets");
        }
    }
}
