using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RelationshipWorkerService.Migrations
{
    /// <inheritdoc />
    public partial class updatedPromptResponseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RespondedAt",
                table: "prompt_responses",
                newName: "ResponseAt");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "prompt_responses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "prompt_responses");

            migrationBuilder.RenameColumn(
                name: "ResponseAt",
                table: "prompt_responses",
                newName: "RespondedAt");
        }
    }
}
