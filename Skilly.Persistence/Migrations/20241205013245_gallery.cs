using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class gallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "servicesgalleries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    galleryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deliverytime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    serviceProviderId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicesgalleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_servicesgalleries_serviceProviders_serviceProviderId",
                        column: x => x.serviceProviderId,
                        principalTable: "serviceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "galleryImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Video = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    galleryId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_galleryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_galleryImages_servicesgalleries_galleryId",
                        column: x => x.galleryId,
                        principalTable: "servicesgalleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_galleryImages_galleryId",
                table: "galleryImages",
                column: "galleryId");

            migrationBuilder.CreateIndex(
                name: "IX_servicesgalleries_serviceProviderId",
                table: "servicesgalleries",
                column: "serviceProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "galleryImages");

            migrationBuilder.DropTable(
                name: "servicesgalleries");
        }
    }
}
