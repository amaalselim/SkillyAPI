using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class providerid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceProviderId",
                table: "requestServices",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "providerId",
                table: "requestServices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_requestServices_ServiceProviderId",
                table: "requestServices",
                column: "ServiceProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_requestServices_serviceProviders_ServiceProviderId",
                table: "requestServices",
                column: "ServiceProviderId",
                principalTable: "serviceProviders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_requestServices_serviceProviders_ServiceProviderId",
                table: "requestServices");

            migrationBuilder.DropIndex(
                name: "IX_requestServices_ServiceProviderId",
                table: "requestServices");

            migrationBuilder.DropColumn(
                name: "ServiceProviderId",
                table: "requestServices");

            migrationBuilder.DropColumn(
                name: "providerId",
                table: "requestServices");
        }
    }
}
