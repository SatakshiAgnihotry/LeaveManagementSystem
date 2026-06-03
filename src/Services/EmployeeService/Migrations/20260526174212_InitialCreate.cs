using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ManagerId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<string>(type: "text", nullable: false),
                    LeaveType = table.Column<string>(type: "text", nullable: false),
                    TotalAllocated = table.Column<int>(type: "integer", nullable: false),
                    Used = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "CreatedAt", "Email", "ManagerId", "Name", "Role" },
                values: new object[,]
                {
                    { "usr-emp-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "memp@gmail.com", "usr-mgr-001", "Arav", "Emp" },
                    { "usr-mgr-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mgr@gmail.com", null, "Mini", "Manager" }
                });

            migrationBuilder.InsertData(
                table: "LeaveBalances",
                columns: new[] { "Id", "EmployeeId", "LeaveType", "TotalAllocated", "Used" },
                values: new object[,]
                {
                    { 1, "usr-mgr-001", "Casual", 15, 0 },
                    { 2, "usr-mgr-001", "Sick", 15, 0 },
                    { 3, "usr-mgr-001", "Privilege", 15, 0 },
                    { 4, "usr-emp-001", "Casual", 15, 0 },
                    { 5, "usr-emp-001", "Sick", 15, 0 },
                    { 6, "usr-emp-001", "Privilege", 15, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId",
                table: "LeaveBalances",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveBalances");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
