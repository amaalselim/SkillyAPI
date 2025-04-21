using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_chatRooms_ChatRoomId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "chatRooms");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatRoomId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ChatRoomId",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChatRoomId",
                table: "Messages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "chatRooms",
                columns: table => new
                {
                    ChatRoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserIds = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatRooms", x => x.ChatRoomId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatRoomId",
                table: "Messages",
                column: "ChatRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_chatRooms_ChatRoomId",
                table: "Messages",
                column: "ChatRoomId",
                principalTable: "chatRooms",
                principalColumn: "ChatRoomId");
        }
    }
}
