using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSAMate.API.Migrations
{
    /// <inheritdoc />
    public partial class QuestionIdRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "Questions",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Questions",
                newName: "id");
        }
    }
}
