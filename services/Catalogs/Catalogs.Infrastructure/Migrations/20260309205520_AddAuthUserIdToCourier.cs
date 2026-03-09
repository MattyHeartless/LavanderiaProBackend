using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthUserIdToCourier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthUserId",
                table: "Couriers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthUserId",
                table: "Couriers");
        }
    }
}
