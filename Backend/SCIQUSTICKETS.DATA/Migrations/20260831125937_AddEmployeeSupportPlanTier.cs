using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSupportPlanTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SupportPlanId",
                table: "Employees",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SupportPlanId",
                table: "Employees",
                column: "SupportPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_SupportPlans_SupportPlanId",
                table: "Employees",
                column: "SupportPlanId",
                principalTable: "SupportPlans",
                principalColumn: "SupportPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_SupportPlans_SupportPlanId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_SupportPlanId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SupportPlanId",
                table: "Employees");
        }
    }
}
