using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class adduserid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "userId",
                table: "offerSalaries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_offerSalaries_userId",
                table: "offerSalaries",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_AspNetUsers_userId",
                table: "offerSalaries",
                column: "userId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_AspNetUsers_userId",
                table: "offerSalaries");

            migrationBuilder.DropIndex(
                name: "IX_offerSalaries_userId",
                table: "offerSalaries");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "offerSalaries");
        }
    }
}
