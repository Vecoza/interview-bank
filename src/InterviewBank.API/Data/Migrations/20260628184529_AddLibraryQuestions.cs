using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewBank.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibraryQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExpectedAnswer = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryQuestions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryQuestions_Difficulty",
                table: "LibraryQuestions",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryQuestions_TopicName",
                table: "LibraryQuestions",
                column: "TopicName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryQuestions");
        }
    }
}
