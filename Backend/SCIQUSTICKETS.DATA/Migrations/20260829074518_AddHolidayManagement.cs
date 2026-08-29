using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
	/// <inheritdoc />
	public partial class AddHolidayManagement : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Holiday",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
					Name = table.Column<string>(type: "longtext", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
					IsRecurringYearly = table.Column<bool>(type: "tinyint(1)", nullable: false),
					Description = table.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
					IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Holiday", x => x.Id);
				})
				.Annotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.CreateTable(
				name: "HolidayConfirmation",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
					HolidayId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
					EmployeeId = table.Column<string>(type: "varchar(255)", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					Status = table.Column<string>(type: "longtext", nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					RespondedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
					IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_HolidayConfirmation", x => x.Id);
					table.ForeignKey(
						name: "FK_HolidayConfirmation_Employees_EmployeeId",
						column: x => x.EmployeeId,
						principalTable: "Employees",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_HolidayConfirmation_Holiday_HolidayId",
						column: x => x.HolidayId,
						principalTable: "Holiday",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				})
				.Annotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.CreateIndex(
				name: "IX_HolidayConfirmation_EmployeeId",
				table: "HolidayConfirmation",
				column: "EmployeeId");

			migrationBuilder.CreateIndex(
				name: "IX_HolidayConfirmation_HolidayId",
				table: "HolidayConfirmation",
				column: "HolidayId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "HolidayConfirmation");

			migrationBuilder.DropTable(
				name: "Holiday");
		}
	}
}