using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_User_UserProfile",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_providerServices_ProviderServiceId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_requestServices_RequestServiceId",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payments",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_UserProfile",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "UserProfile",
                table: "payments");

            migrationBuilder.RenameTable(
                name: "payments",
                newName: "Payment");

            migrationBuilder.RenameIndex(
                name: "IX_payments_RequestServiceId",
                table: "Payment",
                newName: "IX_Payment_RequestServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_ProviderServiceId",
                table: "Payment",
                newName: "IX_Payment_ProviderServiceId");

            migrationBuilder.AddColumn<string>(
                name: "UserImg",
                table: "Review",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_UserId",
                table: "Payment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_User_UserId",
                table: "Payment",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_providerServices_ProviderServiceId",
                table: "Payment",
                column: "ProviderServiceId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_requestServices_RequestServiceId",
                table: "Payment",
                column: "RequestServiceId",
                principalTable: "requestServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_User_UserId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_providerServices_ProviderServiceId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_requestServices_RequestServiceId",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_UserId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "UserImg",
                table: "Review");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "payments");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_RequestServiceId",
                table: "payments",
                newName: "IX_payments_RequestServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_ProviderServiceId",
                table: "payments",
                newName: "IX_payments_ProviderServiceId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "payments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserProfile",
                table: "payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_payments",
                table: "payments",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_payments_providerServices_ProviderServiceId",
                table: "payments",
                column: "ProviderServiceId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_requestServices_RequestServiceId",
                table: "payments",
                column: "RequestServiceId",
                principalTable: "requestServices",
                principalColumn: "Id");
        }
    }
}
