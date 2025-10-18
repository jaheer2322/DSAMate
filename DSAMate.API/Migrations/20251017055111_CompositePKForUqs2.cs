using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSAMate.API.Migrations
{
    /// <inheritdoc />
    public partial class CompositePKForUqs2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserQuestionStatuses_UserId_QuestionId",
                table: "UserQuestionStatuses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuestionStatuses",
                table: "UserQuestionStatuses",
                columns: new[] { "UserId", "QuestionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuestionStatuses",
                table: "UserQuestionStatuses");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionStatuses_UserId_QuestionId",
                table: "UserQuestionStatuses",
                columns: new[] { "UserId", "QuestionId" },
                unique: true);
        }
    }
}
