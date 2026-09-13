using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Notifications.API.Data;

#nullable disable

namespace Notifications.API.Migrations
{
    [DbContext(typeof(NotificationsDbContext))]
    [Migration("20260912230000_AddSmsNotificationOutbox")]
    public partial class AddSmsNotificationOutbox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmsNotificationOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_SmsNotificationOutbox", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotificationOutbox_OrderId_CourierId",
                table: "SmsNotificationOutbox",
                columns: new[] { "OrderId", "CourierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotificationOutbox_SentAt_NextAttemptAt_CreatedAt",
                table: "SmsNotificationOutbox",
                columns: new[] { "SentAt", "NextAttemptAt", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "SmsNotificationOutbox");
    }
}
