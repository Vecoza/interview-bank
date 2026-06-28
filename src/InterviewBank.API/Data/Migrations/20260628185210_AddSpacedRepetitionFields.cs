using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewBank.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpacedRepetitionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EaseFactor",
                table: "Questions",
                type: "double precision",
                nullable: false,
                defaultValue: 2.5);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextReviewAt",
                table: "Questions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SrInterval",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SrRepetitions",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_NextReviewAt",
                table: "Questions",
                column: "NextReviewAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_NextReviewAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "EaseFactor",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "NextReviewAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SrInterval",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "SrRepetitions",
                table: "Questions");
        }
    }
}
