using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addcatt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CategoryId",
                table: "emergencyRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_emergencyRequests_CategoryId",
                table: "emergencyRequests",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_emergencyRequests_categories_CategoryId",
                table: "emergencyRequests",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_emergencyRequests_categories_CategoryId",
                table: "emergencyRequests");

            migrationBuilder.DropIndex(
                name: "IX_emergencyRequests_CategoryId",
                table: "emergencyRequests");

            migrationBuilder.AlterColumn<string>(
                name: "CategoryId",
                table: "emergencyRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
