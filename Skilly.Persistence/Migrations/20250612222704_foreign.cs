using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class foreign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "emergencyRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedProviderId",
                table: "emergencyRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_emergencyRequests_AssignedProviderId",
                table: "emergencyRequests",
                column: "AssignedProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_emergencyRequests_UserId",
                table: "emergencyRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_emergencyRequests_User_AssignedProviderId",
                table: "emergencyRequests",
                column: "AssignedProviderId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_emergencyRequests_User_UserId",
                table: "emergencyRequests",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_emergencyRequests_User_AssignedProviderId",
                table: "emergencyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_emergencyRequests_User_UserId",
                table: "emergencyRequests");

            migrationBuilder.DropIndex(
                name: "IX_emergencyRequests_AssignedProviderId",
                table: "emergencyRequests");

            migrationBuilder.DropIndex(
                name: "IX_emergencyRequests_UserId",
                table: "emergencyRequests");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "emergencyRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedProviderId",
                table: "emergencyRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
