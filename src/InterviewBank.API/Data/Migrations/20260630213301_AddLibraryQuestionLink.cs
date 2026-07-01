using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewBank.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryQuestionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LibraryQuestionId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_LibraryQuestionId",
                table: "Questions",
                column: "LibraryQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_UserId_LibraryQuestionId",
                table: "Questions",
                columns: new[] { "UserId", "LibraryQuestionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_LibraryQuestions_LibraryQuestionId",
                table: "Questions",
                column: "LibraryQuestionId",
                principalTable: "LibraryQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_LibraryQuestions_LibraryQuestionId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_LibraryQuestionId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_UserId_LibraryQuestionId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "LibraryQuestionId",
                table: "Questions");
        }
    }
}
