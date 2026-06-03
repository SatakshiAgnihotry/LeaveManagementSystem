using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Migrations
{
    /// <inheritdoc />
    public partial class CreateIdentityServiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "usr-emp-001",
                column: "PasswordHash",
                value: "$2a$11$n68rB4cpeBO5fKMf9J3/r.GeAembomFybQw60iN03noL/mk9TkVa6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "usr-mgr-001",
                column: "PasswordHash",
                value: "$2a$11$ddxDB93dEXdHWJuz6uNLoOQFQMTTk9ebSuoahM8AOOMIa5iIhOhdi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "usr-emp-001",
                column: "PasswordHash",
                value: "$2a$11$aVCuiuIznRADv0n3nhaGaOeSfetYMJvwnDoVjCCu/GG0iRh6bf2yK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: "usr-mgr-001",
                column: "PasswordHash",
                value: "$2a$11$VdR7EKaWiS0o/V1Sht9nyOWOfuTv1F6PVJv1eDRZpqhqPFKCCuoC2");
        }
    }
}
