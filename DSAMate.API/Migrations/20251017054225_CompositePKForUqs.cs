using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSAMate.API.Migrations
{
    /// <inheritdoc />
    public partial class CompositePKForUqs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuestionStatuses",
                table: "UserQuestionStatuses");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserQuestionStatuses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "UserQuestionStatuses",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuestionStatuses",
                table: "UserQuestionStatuses",
                column: "Id");
        }
    }
}
