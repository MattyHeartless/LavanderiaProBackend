using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThemeIcon",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThemeIcon",
                table: "Services");
        }
    }
}
