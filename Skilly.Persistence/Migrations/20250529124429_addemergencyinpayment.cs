
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addemergencyinpayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmergencyRequestId",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_EmergencyRequestId",
                table: "Payment",
                column: "EmergencyRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_emergencyRequests_EmergencyRequestId",
                table: "Payment",
                column: "EmergencyRequestId",
                principalTable: "emergencyRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_emergencyRequests_EmergencyRequestId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_EmergencyRequestId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "EmergencyRequestId",
                table: "Payment");
        }
    }
}
