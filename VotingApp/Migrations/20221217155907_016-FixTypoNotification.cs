using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingApp.Migrations
{
    /// <inheritdoc />
    public partial class _016FixTypoNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Idea_IdeaId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "IdeadId",
                table: "Notification");

            migrationBuilder.AlterColumn<int>(
                name: "IdeaId",
                table: "Notification",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Idea_IdeaId",
                table: "Notification",
                column: "IdeaId",
                principalTable: "Idea",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Idea_IdeaId",
                table: "Notification");

            migrationBuilder.AlterColumn<int>(
                name: "IdeaId",
                table: "Notification",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "IdeadId",
                table: "Notification",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Idea_IdeaId",
                table: "Notification",
                column: "IdeaId",
                principalTable: "Idea",
                principalColumn: "Id");
        }
    }
}
