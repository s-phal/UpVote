using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingApp.Migrations
{
    /// <inheritdoc />
    public partial class _004AddMemberIdColumnToVoteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberId",
                table: "Vote",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_MemberId",
                table: "Vote",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_AspNetUsers_MemberId",
                table: "Vote",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vote_AspNetUsers_MemberId",
                table: "Vote");

            migrationBuilder.DropIndex(
                name: "IX_Vote_MemberId",
                table: "Vote");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Vote");
        }
    }
}
