using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DSAMate.API.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class NewMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "37798ece-bdaa-4764-9450-c2dcf7548a2f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cf330825-ba85-43bc-9d1b-3d52286bc775");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "37798ece-bdaa-4764-9450-c2dcf7548a1f", "37798ece-bdaa-4764-9450-c2dcf7548a1f", "Admin", "ADMIN" },
                    { "cf330825-ba85-43bc-9d1b-3d52286bc776", "cf330825-ba85-43bc-9d1b-3d52286bc776", "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "37798ece-bdaa-4764-9450-c2dcf7548a1f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cf330825-ba85-43bc-9d1b-3d52286bc776");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "37798ece-bdaa-4764-9450-c2dcf7548a2f", "37798ece-bdaa-4764-9450-c2dcf7548a2f", "Admin", "ADMIN" },
                    { "cf330825-ba85-43bc-9d1b-3d52286bc775", "cf330825-ba85-43bc-9d1b-3d52286bc775", "User", "USER" }
                });
        }
    }
}
