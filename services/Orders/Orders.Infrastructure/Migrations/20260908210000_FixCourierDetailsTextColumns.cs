using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Orders.Infrastructure.Persistence;

#nullable disable

namespace Orders.Infrastructure.Migrations;

/// <summary>
/// Repairs the database schema created by AddCourierDetails, which persisted
/// the courier display fields as integers while the domain model uses text.
/// </summary>
[DbContext(typeof(OrdersDbContext))]
[Migration("20260908210000_FixCourierDetailsTextColumns")]
public partial class FixCourierDetailsTextColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "CourierName",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "CourierPhone",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "CourierName",
            table: "Orders",
            type: "int",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<int>(
            name: "CourierPhone",
            table: "Orders",
            type: "int",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);
    }
}
