using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Notifications.API.Data;

#nullable disable

namespace Notifications.API.Migrations;

[DbContext(typeof(NotificationsDbContext))]
[Migration("20260916150001_AddClientOrderStatusSmsEvents")]
public partial class AddClientOrderStatusSmsEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_SmsNotificationOutbox_OrderId_CourierId", table: "SmsNotificationOutbox");

        migrationBuilder.AlterColumn<Guid>(
            name: "CourierId",
            table: "SmsNotificationOutbox",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AddColumn<string>(
            name: "EventType",
            table: "SmsNotificationOutbox",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "CourierNewOrder");

        migrationBuilder.CreateIndex(
            name: "IX_SmsNotificationOutbox_OrderId_EventType_CourierId",
            table: "SmsNotificationOutbox",
            columns: new[] { "OrderId", "EventType", "CourierId" },
            unique: true);

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_SmsNotificationOutbox_OrderId_EventType_CourierId", table: "SmsNotificationOutbox");

        migrationBuilder.DropColumn(name: "EventType", table: "SmsNotificationOutbox");

        migrationBuilder.AlterColumn<Guid>(
            name: "CourierId",
            table: "SmsNotificationOutbox",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_SmsNotificationOutbox_OrderId_CourierId",
            table: "SmsNotificationOutbox",
            columns: new[] { "OrderId", "CourierId" },
            unique: true);
    }
}
