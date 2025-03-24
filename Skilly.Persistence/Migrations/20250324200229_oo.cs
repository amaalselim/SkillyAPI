using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class oo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_providerServices_serviceId",
                table: "offerSalaries");

            migrationBuilder.AddColumn<string>(
                name: "ProviderServicesId",
                table: "offerSalaries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_offerSalaries_ProviderServicesId",
                table: "offerSalaries",
                column: "ProviderServicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_providerServices_ProviderServicesId",
                table: "offerSalaries",
                column: "ProviderServicesId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_providerServices_serviceId",
                table: "offerSalaries",
                column: "serviceId",
                principalTable: "providerServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_providerServices_ProviderServicesId",
                table: "offerSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_providerServices_serviceId",
                table: "offerSalaries");

            migrationBuilder.DropIndex(
                name: "IX_offerSalaries_ProviderServicesId",
                table: "offerSalaries");

            migrationBuilder.DropColumn(
                name: "ProviderServicesId",
                table: "offerSalaries");

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_providerServices_serviceId",
                table: "offerSalaries",
                column: "serviceId",
                principalTable: "providerServices",
                principalColumn: "Id");
        }
    }
}
