using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class SupportPlanTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignmentStrategy",
                table: "SupportPlans",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultPriorityId",
                table: "SupportPlans",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "EscalationLevel",
                table: "SupportPlans",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IncludesWeekendSupport",
                table: "SupportPlans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SupportHours",
                table: "SupportPlans",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountDedicatedEmployees",
                columns: table => new
                {
                    AccountDedicatedEmployeeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmployeeUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountDedicatedEmployees", x => x.AccountDedicatedEmployeeId);
                    table.ForeignKey(
                        name: "FK_AccountDedicatedEmployees_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountDedicatedEmployees_Employees_EmployeeUserId",
                        column: x => x.EmployeeUserId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDedicatedEmployees_AccountId",
                table: "AccountDedicatedEmployees",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDedicatedEmployees_EmployeeUserId",
                table: "AccountDedicatedEmployees",
                column: "EmployeeUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountDedicatedEmployees");

            migrationBuilder.DropColumn(
                name: "AssignmentStrategy",
                table: "SupportPlans");

            migrationBuilder.DropColumn(
                name: "DefaultPriorityId",
                table: "SupportPlans");

            migrationBuilder.DropColumn(
                name: "EscalationLevel",
                table: "SupportPlans");

            migrationBuilder.DropColumn(
                name: "IncludesWeekendSupport",
                table: "SupportPlans");

            migrationBuilder.DropColumn(
                name: "SupportHours",
                table: "SupportPlans");
        }
    }
}
