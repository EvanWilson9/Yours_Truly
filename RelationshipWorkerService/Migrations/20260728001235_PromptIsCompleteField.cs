using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RelationshipWorkerService.Migrations
{
    /// <inheritdoc />
    public partial class PromptIsCompleteField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "prompts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComplete",
                table: "prompts");
        }
    }
}
