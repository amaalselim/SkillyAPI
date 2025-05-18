using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skilly.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class useduscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "useDiscount",
                table: "userProfiles",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "useDiscount",
                table: "userProfiles");
        }
    }
}
