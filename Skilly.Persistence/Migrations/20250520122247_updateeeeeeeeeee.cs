using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateeeeeeeeeee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_providerServices_userProfiles_userprofileId",
                table: "providerServices");

            migrationBuilder.AddForeignKey(
                name: "FK_providerServices_User_userprofileId",
                table: "providerServices",
                column: "userprofileId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_providerServices_User_userprofileId",
                table: "providerServices");

            migrationBuilder.AddForeignKey(
                name: "FK_providerServices_userProfiles_userprofileId",
                table: "providerServices",
                column: "userprofileId",
                principalTable: "userProfiles",
                principalColumn: "Id");
        }
    }
}
