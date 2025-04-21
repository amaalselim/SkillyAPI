using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class msg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_providerServices_ProviderServicesId",
                table: "offerSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_providerServices_serviceId",
                table: "offerSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_requestServices_requestserviceId",
                table: "offerSalaries");

            migrationBuilder.DropIndex(
                name: "IX_offerSalaries_ProviderServicesId",
                table: "offerSalaries");

            migrationBuilder.DropColumn(
                name: "ProviderServicesId",
                table: "offerSalaries");

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ok = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_providerServices_serviceId",
                table: "offerSalaries",
                column: "serviceId",
                principalTable: "providerServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_requestServices_requestserviceId",
                table: "offerSalaries",
                column: "requestserviceId",
                principalTable: "requestServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_providerServices_serviceId",
                table: "offerSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_requestServices_requestserviceId",
                table: "offerSalaries");

            migrationBuilder.DropTable(
                name: "Messages");

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

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_requestServices_requestserviceId",
                table: "offerSalaries",
                column: "requestserviceId",
                principalTable: "requestServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
