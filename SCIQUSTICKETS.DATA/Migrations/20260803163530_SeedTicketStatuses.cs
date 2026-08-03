using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class SeedTicketStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TicketStatuses",
                columns: new[] { "TicketStatusId", "CreatedDate", "Description", "IsClosed", "IsDeleted", "LastUpdatedDate", "Name", "Status" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "New ticket created", false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Open", true },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ticket is being worked on", false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "In Progress", true },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Waiting for additional information", false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pending", true },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Solution provided", false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Resolved", true },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ticket closed successfully", true, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Closed", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TicketStatuses",
                keyColumn: "TicketStatusId",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "TicketStatuses",
                keyColumn: "TicketStatusId",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "TicketStatuses",
                keyColumn: "TicketStatusId",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "TicketStatuses",
                keyColumn: "TicketStatusId",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "TicketStatuses",
                keyColumn: "TicketStatusId",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));
        }
    }
}
