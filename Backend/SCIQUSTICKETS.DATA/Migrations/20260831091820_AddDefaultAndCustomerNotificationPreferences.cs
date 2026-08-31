using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultAndCustomerNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Category = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotificationPreferences", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DefaultEmployeeEmailNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReceiveAll = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Assignment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Acceptance = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Rejection = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Expiry = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Reassignment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StatusChange = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Closure = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Reopen = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultEmployeeEmailNotificationPreferences", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationPreferences_Category",
                table: "CustomerNotificationPreferences",
                column: "Category",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerNotificationPreferences");

            migrationBuilder.DropTable(
                name: "DefaultEmployeeEmailNotificationPreferences");
        }
    }
}
