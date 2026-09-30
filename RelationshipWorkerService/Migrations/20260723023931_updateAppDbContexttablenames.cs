using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RelationshipWorkerService.Migrations
{
    /// <inheritdoc />
    public partial class updateAppDbContexttablenames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromptResponses_Prompts_PromptId",
                table: "PromptResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_PromptResponses_Users_UserId",
                table: "PromptResponses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prompts",
                table: "Prompts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromptResponses",
                table: "PromptResponses");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Prompts",
                newName: "prompts");

            migrationBuilder.RenameTable(
                name: "PromptResponses",
                newName: "prompt_responses");

            migrationBuilder.RenameIndex(
                name: "IX_PromptResponses_UserId",
                table: "prompt_responses",
                newName: "IX_prompt_responses_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PromptResponses_PromptId",
                table: "prompt_responses",
                newName: "IX_prompt_responses_PromptId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_prompts",
                table: "prompts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_prompt_responses",
                table: "prompt_responses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_prompt_responses_prompts_PromptId",
                table: "prompt_responses",
                column: "PromptId",
                principalTable: "prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_prompt_responses_users_UserId",
                table: "prompt_responses",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prompt_responses_prompts_PromptId",
                table: "prompt_responses");

            migrationBuilder.DropForeignKey(
                name: "FK_prompt_responses_users_UserId",
                table: "prompt_responses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_prompts",
                table: "prompts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_prompt_responses",
                table: "prompt_responses");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "prompts",
                newName: "Prompts");

            migrationBuilder.RenameTable(
                name: "prompt_responses",
                newName: "PromptResponses");

            migrationBuilder.RenameIndex(
                name: "IX_prompt_responses_UserId",
                table: "PromptResponses",
                newName: "IX_PromptResponses_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_prompt_responses_PromptId",
                table: "PromptResponses",
                newName: "IX_PromptResponses_PromptId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prompts",
                table: "Prompts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromptResponses",
                table: "PromptResponses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromptResponses_Prompts_PromptId",
                table: "PromptResponses",
                column: "PromptId",
                principalTable: "Prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromptResponses_Users_UserId",
                table: "PromptResponses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
