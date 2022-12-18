using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingApp.Migrations
{
    /// <inheritdoc />
    public partial class _019UpdatedNotificationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AspNetUsers_MemberId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "IdeaOwnerId",
                table: "Notification");

            migrationBuilder.AlterColumn<string>(
                name: "MemberId",
                table: "Notification",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AspNetUsers_MemberId",
                table: "Notification",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AspNetUsers_MemberId",
                table: "Notification");

            migrationBuilder.AlterColumn<string>(
                name: "MemberId",
                table: "Notification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "IdeaOwnerId",
                table: "Notification",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AspNetUsers_MemberId",
                table: "Notification",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
