using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addrequestservice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "requestId",
                table: "Review",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Review_requestId",
                table: "Review",
                column: "requestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_requestServices_requestId",
                table: "Review",
                column: "requestId",
                principalTable: "requestServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_requestServices_requestId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Review_requestId",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "requestId",
                table: "Review");
        }
    }
}
