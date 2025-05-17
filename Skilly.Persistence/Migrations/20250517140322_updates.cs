using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_providerServices_ProviderServiceId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_requestServices_RequestServiceId",
                table: "payments");

            migrationBuilder.AlterColumn<string>(
                name: "RequestServiceId",
                table: "payments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderServiceId",
                table: "payments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_providerServices_ProviderServiceId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_requestServices_RequestServiceId",
                table: "payments");

            migrationBuilder.AlterColumn<string>(
                name: "RequestServiceId",
                table: "payments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderServiceId",
                table: "payments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_providerServices_ProviderServiceId",
                table: "payments",
                column: "ProviderServiceId",
                principalTable: "providerServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_requestServices_RequestServiceId",
                table: "payments",
                column: "RequestServiceId",
                principalTable: "requestServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
