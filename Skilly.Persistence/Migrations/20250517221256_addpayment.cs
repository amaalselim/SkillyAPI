using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addpayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserProfile",
                table: "payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_UserProfile",
                table: "payments",
                column: "UserProfile");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_User_UserProfile",
                table: "payments",
                column: "UserProfile",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_User_UserProfile",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_UserProfile",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "UserProfile",
                table: "payments");
        }
    }
}
