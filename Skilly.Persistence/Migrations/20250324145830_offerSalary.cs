using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class offerSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "offerSalaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Deliverytime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    serviceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    requestserviceId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offerSalaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_offerSalaries_providerServices_serviceId",
                        column: x => x.serviceId,
                        principalTable: "providerServices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_offerSalaries_requestServices_requestserviceId",
                        column: x => x.requestserviceId,
                        principalTable: "requestServices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_offerSalaries_requestserviceId",
                table: "offerSalaries",
                column: "requestserviceId");

            migrationBuilder.CreateIndex(
                name: "IX_offerSalaries_serviceId",
                table: "offerSalaries",
                column: "serviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "offerSalaries");
        }
    }
}
