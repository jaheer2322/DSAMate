using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSAMate.API.Migrations
{
    /// <inheritdoc />
    public partial class Indexings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Questions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Difficulty",
                table: "Questions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Difficulty",
                table: "Questions",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Topic",
                table: "Questions",
                column: "Topic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Difficulty",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Topic",
                table: "Questions");

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Difficulty",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
