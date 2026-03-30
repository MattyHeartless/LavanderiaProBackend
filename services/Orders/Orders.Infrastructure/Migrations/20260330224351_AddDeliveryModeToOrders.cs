using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryModeToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryEtaHours",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryModeCode",
                table: "Orders",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryModeId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryModeName",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryModeSurcharge",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeliveryModes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EtaHours = table.Column<int>(type: "int", nullable: false),
                    SurchargeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryModes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DeliveryModes",
                columns: new[] { "Id", "Code", "EtaHours", "IsActive", "Name", "SortOrder", "SurchargeAmount" },
                values: new object[,]
                {
                    { 1, "EXPRESS_3H", 3, true, "Tres horas (Express)", 1, 80m },
                    { 2, "SIX_HOURS", 6, true, "Seis horas", 2, 50m },
                    { 3, "TWELVE_HOURS", 12, true, "Doce horas", 3, 25m },
                    { 4, "TWENTY_FOUR_HOURS", 24, true, "24 horas", 4, 0m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryModeId",
                table: "Orders",
                column: "DeliveryModeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryModes_Code",
                table: "DeliveryModes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryModes_DeliveryModeId",
                table: "Orders",
                column: "DeliveryModeId",
                principalTable: "DeliveryModes",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
                UPDATE Orders
                SET DeliveryModeId = 4,
                    DeliveryModeCode = 'TWENTY_FOUR_HOURS',
                    DeliveryModeName = '24 horas',
                    DeliveryEtaHours = 24,
                    DeliveryModeSurcharge = 0
                WHERE DeliveryModeId IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryModes_DeliveryModeId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DeliveryModes");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveryModeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryEtaHours",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryModeCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryModeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryModeName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryModeSurcharge",
                table: "Orders");
        }
    }
}
