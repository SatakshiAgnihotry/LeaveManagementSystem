using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IdentityService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ManagerId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "ManagerId", "Name", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { "usr-emp-001", "memp@gmail.com", "usr-mgr-001", "Arav", "$2a$11$aVCuiuIznRADv0n3nhaGaOeSfetYMJvwnDoVjCCu/GG0iRh6bf2yK", "Emp" },
                    { "usr-mgr-001", "mgr@gmail.com", null, "Mini", "$2a$11$VdR7EKaWiS0o/V1Sht9nyOWOfuTv1F6PVJv1eDRZpqhqPFKCCuoC2", "Manager" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
