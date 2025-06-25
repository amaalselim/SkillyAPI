using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class edit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_providerServices_serviceId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_requestServices_requestId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_serviceProviders_ProviderId",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Review",
                table: "Review");

            migrationBuilder.RenameTable(
                name: "Review",
                newName: "reviews");

            migrationBuilder.RenameIndex(
                name: "IX_Review_serviceId",
                table: "reviews",
                newName: "IX_reviews_serviceId");

            migrationBuilder.RenameIndex(
                name: "IX_Review_requestId",
                table: "reviews",
                newName: "IX_reviews_requestId");

            migrationBuilder.RenameIndex(
                name: "IX_Review_ProviderId",
                table: "reviews",
                newName: "IX_reviews_ProviderId");

            migrationBuilder.AddColumn<string>(
                name: "ServiceProviderId",
                table: "reviews",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_reviews",
                table: "reviews",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_ServiceProviderId",
                table: "reviews",
                column: "ServiceProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_User_ProviderId",
                table: "reviews",
                column: "ProviderId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_providerServices_serviceId",
                table: "reviews",
                column: "serviceId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_requestServices_requestId",
                table: "reviews",
                column: "requestId",
                principalTable: "requestServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_serviceProviders_ServiceProviderId",
                table: "reviews",
                column: "ServiceProviderId",
                principalTable: "serviceProviders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reviews_User_ProviderId",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_providerServices_serviceId",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_requestServices_requestId",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_serviceProviders_ServiceProviderId",
                table: "reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reviews",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_ServiceProviderId",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "ServiceProviderId",
                table: "reviews");

            migrationBuilder.RenameTable(
                name: "reviews",
                newName: "Review");

            migrationBuilder.RenameIndex(
                name: "IX_reviews_serviceId",
                table: "Review",
                newName: "IX_Review_serviceId");

            migrationBuilder.RenameIndex(
                name: "IX_reviews_requestId",
                table: "Review",
                newName: "IX_Review_requestId");

            migrationBuilder.RenameIndex(
                name: "IX_reviews_ProviderId",
                table: "Review",
                newName: "IX_Review_ProviderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Review",
                table: "Review",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_providerServices_serviceId",
                table: "Review",
                column: "serviceId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_requestServices_requestId",
                table: "Review",
                column: "requestId",
                principalTable: "requestServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_serviceProviders_ProviderId",
                table: "Review",
                column: "ProviderId",
                principalTable: "serviceProviders",
                principalColumn: "Id");
        }
    }
}
