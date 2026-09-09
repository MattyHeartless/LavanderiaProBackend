using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourierAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AvailabilityUpdatedAt",
                table: "Couriers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Couriers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityUpdatedAt",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Couriers");
        }
    }
}
