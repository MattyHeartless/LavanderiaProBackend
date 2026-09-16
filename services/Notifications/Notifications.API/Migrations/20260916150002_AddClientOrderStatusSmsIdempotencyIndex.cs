using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Notifications.API.Data;

#nullable disable

namespace Notifications.API.Migrations;

[DbContext(typeof(NotificationsDbContext))]
[Migration("20260916150002_AddClientOrderStatusSmsIdempotencyIndex")]
public partial class AddClientOrderStatusSmsIdempotencyIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_SmsNotificationOutbox_ClientOrderEvent",
            table: "SmsNotificationOutbox",
            columns: new[] { "OrderId", "EventType" },
            unique: true,
            filter: "[CourierId] IS NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropIndex(name: "IX_SmsNotificationOutbox_ClientOrderEvent", table: "SmsNotificationOutbox");
}
