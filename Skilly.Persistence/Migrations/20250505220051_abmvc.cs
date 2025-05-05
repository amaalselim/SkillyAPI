using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class abmvc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profession",
                table: "serviceProviders");

            migrationBuilder.AddColumn<string>(
                name: "ProfessionName",
                table: "categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfessionName",
                table: "categories");

            migrationBuilder.AddColumn<string>(
                name: "profession",
                table: "serviceProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
