using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceCA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedAndDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-user-admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "seed-concurrency-stamp", "AQAAAAIAAYagAAAAEGSwk2grLuyQCyk8zt1BJ/FMmpsyiobc0jS4F3no0KDF9sWHAy9RYF4thm1/U6YG0Q==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-user-admin",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "2db6d348-e7e2-4c11-aa92-06d0636fb853", "AQAAAAIAAYagAAAAEDmfAPIty25+rZh2SclMIjibeqlY2ewehidhSElmCWdL3PMCYr7kf4I88SeFde10zw==" });
        }
    }
}
