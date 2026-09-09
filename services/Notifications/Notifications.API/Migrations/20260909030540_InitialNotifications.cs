using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notifications.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourierPushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    P256dh = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ContentEncoding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourierPushSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourierPushSubscriptions_AuthUserId_IsAvailable_IsEnabled",
                table: "CourierPushSubscriptions",
                columns: new[] { "AuthUserId", "IsAvailable", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_CourierPushSubscriptions_Endpoint",
                table: "CourierPushSubscriptions",
                column: "Endpoint",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourierPushSubscriptions");
        }
    }
}
