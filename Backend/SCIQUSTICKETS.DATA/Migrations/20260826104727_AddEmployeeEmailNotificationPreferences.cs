using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeEmailNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeEmailNotificationPreferences",
                columns: table => new
                {
                    EmployeeEmailNotificationPreferenceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmployeeId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReceiveAll = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Assignment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Acceptance = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Rejection = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Expiry = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Reassignment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StatusChange = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Closure = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Reopen = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeEmailNotificationPreferences", x => x.EmployeeEmailNotificationPreferenceId);
                    table.ForeignKey(
                        name: "FK_EmployeeEmailNotificationPreferences_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEmailNotificationPreferences_EmployeeId",
                table: "EmployeeEmailNotificationPreferences",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeEmailNotificationPreferences");
        }
    }
}
