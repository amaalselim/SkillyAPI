using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class adduid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "userprofileId",
                table: "providerServices",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_providerServices_userprofileId",
                table: "providerServices",
                column: "userprofileId");

            migrationBuilder.AddForeignKey(
                name: "FK_providerServices_userProfiles_userprofileId",
                table: "providerServices",
                column: "userprofileId",
                principalTable: "userProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_providerServices_userProfiles_userprofileId",
                table: "providerServices");

            migrationBuilder.DropIndex(
                name: "IX_providerServices_userprofileId",
                table: "providerServices");

            migrationBuilder.DropColumn(
                name: "userprofileId",
                table: "providerServices");
        }
    }
}
