using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingApp.Migrations
{
    /// <inheritdoc />
    public partial class _017RemovedCommentNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Comment_CommentId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_CommentId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "Notification");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentId",
                table: "Notification",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CommentId",
                table: "Notification",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Comment_CommentId",
                table: "Notification",
                column: "CommentId",
                principalTable: "Comment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
