using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileImageAndOrderEvidences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Couriers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "OrderEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEvidences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderEvidences_CreatedAt",
                table: "OrderEvidences",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderEvidences_OrderId",
                table: "OrderEvidences",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderEvidences");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Couriers");
        }
    }
}
