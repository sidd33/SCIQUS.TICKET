using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIQUSTICKETS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomRoutingAndAssignmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcceptanceDeadlineHours",
                table: "TicketSubTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ManualOnly",
                table: "TicketSubTypes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAcceptance",
                table: "TicketSubTypes",
                type: "tinyint(1)",
                nullable: true);

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
                name: "CurrentFallbackAttempt",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Tickets",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsSlaBreached",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OverdueHours",
                table: "Tickets",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ResolutionTimeInHours",
                table: "Tickets",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SlaDueDate",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlaMetStatus",
                table: "Tickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "ManualOnly",
                table: "TicketPriorities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ResponseSlaInHours",
                table: "TicketPriorities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FromDepartmentId",
                table: "TicketAssignments",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoAssigned",
                table: "TicketAssignments",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TicketAssignments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ToDepartmentId",
                table: "TicketAssignments",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

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

            migrationBuilder.CreateTable(
                name: "SlaConfigurations",
                columns: table => new
                {
                    SlaConfigurationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DefaultAutoAssignMethod = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultW_Load = table.Column<double>(type: "double", nullable: false),
                    DefaultW_Severity = table.Column<double>(type: "double", nullable: false),
                    DefaultW_Recency = table.Column<double>(type: "double", nullable: false),
                    DefaultRecencyCapHours = table.Column<int>(type: "int", nullable: false),
                    DefaultMaxConsecutiveAssignments = table.Column<int>(type: "int", nullable: false),
                    DefaultMaxConcurrentOpenTickets = table.Column<int>(type: "int", nullable: false),
                    MaxFallbackAttempts = table.Column<int>(type: "int", nullable: false),
                    AutoClosureHours = table.Column<int>(type: "int", nullable: false),
                    AllowEmployeeReopen = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReopenGraceDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaConfigurations", x => x.SlaConfigurationId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketStateChangeHistories",
                columns: table => new
                {
                    TicketStateChangeHistoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TicketId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChangeType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedByUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStateChangeHistories", x => x.TicketStateChangeHistoryId);
                    table.ForeignKey(
                        name: "FK_TicketStateChangeHistories_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketStateChangeHistories_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "TicketId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DepartmentId",
                table: "Tickets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStateChangeHistories_ChangedByUserId",
                table: "TicketStateChangeHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStateChangeHistories_TicketId",
                table: "TicketStateChangeHistories",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Departments_DepartmentId",
                table: "Tickets",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Departments_DepartmentId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "SlaConfigurations");

            migrationBuilder.DropTable(
                name: "TicketStateChangeHistories");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DepartmentId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AcceptanceDeadlineHours",
                table: "TicketSubTypes");

            migrationBuilder.DropColumn(
                name: "ManualOnly",
                table: "TicketSubTypes");

            migrationBuilder.DropColumn(
                name: "RequiresAcceptance",
                table: "TicketSubTypes");

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
                name: "CurrentFallbackAttempt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsSlaBreached",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "OverdueHours",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ResolutionTimeInHours",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SlaDueDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SlaMetStatus",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ManualOnly",
                table: "TicketPriorities");

            migrationBuilder.DropColumn(
                name: "ResponseSlaInHours",
                table: "TicketPriorities");

            migrationBuilder.DropColumn(
                name: "FromDepartmentId",
                table: "TicketAssignments");

            migrationBuilder.DropColumn(
                name: "IsAutoAssigned",
                table: "TicketAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TicketAssignments");

            migrationBuilder.DropColumn(
                name: "ToDepartmentId",
                table: "TicketAssignments");

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
