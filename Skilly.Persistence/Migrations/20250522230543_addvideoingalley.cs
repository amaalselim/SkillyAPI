using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addvideoingalley : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Img",
                table: "servicesgalleries");

            migrationBuilder.AddColumn<string>(
                name: "video",
                table: "servicesgalleries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "video",
                table: "servicesgalleries");

            migrationBuilder.AddColumn<string>(
                name: "Img",
                table: "servicesgalleries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
