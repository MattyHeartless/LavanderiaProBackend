using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Orders.Infrastructure.Persistence;

#nullable disable

namespace Orders.Infrastructure.Migrations;

[DbContext(typeof(OrdersDbContext))]
[Migration("20260921120000_AddCourierPayments")]
public partial class AddCourierPayments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CourierPayments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CourierGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CourierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                OrdersCount = table.Column<int>(type: "int", nullable: false),
                PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                PaidByAdminId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_CourierPayments", x => x.Id));

        migrationBuilder.CreateTable(
            name: "CourierPaymentOrders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CourierPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CourierPaymentOrders", x => x.Id);
                table.ForeignKey(
                    name: "FK_CourierPaymentOrders_CourierPayments_CourierPaymentId",
                    column: x => x.CourierPaymentId,
                    principalTable: "CourierPayments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CourierPaymentOrders_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CourierPaymentOrders_CourierPaymentId",
            table: "CourierPaymentOrders",
            column: "CourierPaymentId");

        migrationBuilder.CreateIndex(
            name: "IX_CourierPaymentOrders_OrderId",
            table: "CourierPaymentOrders",
            column: "OrderId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CourierPayments_CourierGuid_PaidAt",
            table: "CourierPayments",
            columns: new[] { "CourierGuid", "PaidAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CourierPaymentOrders");
        migrationBuilder.DropTable(name: "CourierPayments");
    }
}
