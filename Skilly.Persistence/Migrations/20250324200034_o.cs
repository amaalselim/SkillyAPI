using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class o : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_requestServices_requestserviceId",
                table: "offerSalaries");

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_requestServices_requestserviceId",
                table: "offerSalaries",
                column: "requestserviceId",
                principalTable: "requestServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_offerSalaries_requestServices_requestserviceId",
                table: "offerSalaries");

            migrationBuilder.AddForeignKey(
                name: "FK_offerSalaries_requestServices_requestserviceId",
                table: "offerSalaries",
                column: "requestserviceId",
                principalTable: "requestServices",
                principalColumn: "Id");
        }
    }
}
