using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class reviewww : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reviews_serviceProviders_ProviderId",
                table: "reviews");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "reviews",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "serviceId",
                table: "reviews",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reviews_serviceId",
                table: "reviews",
                column: "serviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_providerServices_serviceId",
                table: "reviews",
                column: "serviceId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_serviceProviders_ProviderId",
                table: "reviews",
                column: "ProviderId",
                principalTable: "serviceProviders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reviews_providerServices_serviceId",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_serviceProviders_ProviderId",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_serviceId",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "serviceId",
                table: "reviews");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "reviews",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_serviceProviders_ProviderId",
                table: "reviews",
                column: "ProviderId",
                principalTable: "serviceProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
