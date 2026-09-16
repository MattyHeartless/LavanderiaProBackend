using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Orders.Infrastructure.Persistence;

#nullable disable

namespace Orders.Infrastructure.Migrations;

[DbContext(typeof(OrdersDbContext))]
[Migration("20260916150000_AddClientOrderSmsNotificationOutbox")]
public partial class AddClientOrderSmsNotificationOutbox : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ClientOrderSmsNotificationOutbox",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EventType = table.Column<int>(type: "int", nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                Attempts = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ClientOrderSmsNotificationOutbox", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ClientOrderSmsNotificationOutbox_OrderId_EventType",
            table: "ClientOrderSmsNotificationOutbox",
            columns: new[] { "OrderId", "EventType" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ClientOrderSmsNotificationOutbox_ProcessedAt_CreatedAt",
            table: "ClientOrderSmsNotificationOutbox",
            columns: new[] { "ProcessedAt", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "ClientOrderSmsNotificationOutbox");
}
