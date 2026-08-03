using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class FixTicketTransactionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInternal",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RaisedByEmployeeId",
                table: "Tickets",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "Tickets",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_RaisedByEmployeeId",
                table: "Tickets",
                column: "RaisedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Employees_RaisedByEmployeeId",
                table: "Tickets",
                column: "RaisedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Employees_RaisedByEmployeeId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_RaisedByEmployeeId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsInternal",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "RaisedByEmployeeId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "Tickets");
        }
    }
}
