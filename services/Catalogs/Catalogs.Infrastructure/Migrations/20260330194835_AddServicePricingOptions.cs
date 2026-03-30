using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServicePricingOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicePricingOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UoM = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePricingOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePricingOptions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePricingOptions_ServiceId_OptionName",
                table: "ServicePricingOptions",
                columns: new[] { "ServiceId", "OptionName" },
                unique: true);

            // Backfill: create one default PricingOption per existing Service based on its current UoM
            migrationBuilder.Sql(@"
                INSERT INTO ServicePricingOptions (Id, ServiceId, OptionName, Price, UoM, IsActive, CreatedAt, UpdatedAt)
                SELECT
                    NEWID(),
                    Id,
                    CASE UoM
                        WHEN 'KG'    THEN 'Por kilo'
                        WHEN 'PZ'    THEN 'Por pieza'
                        WHEN 'DOC'   THEN 'Por docena'
                        WHEN 'BULTO' THEN 'Bulto mediano'
                        ELSE              'Por pieza'
                    END,
                    Price,
                    CASE UoM
                        WHEN 'KG'    THEN 'KG'
                        WHEN 'PZ'    THEN 'PZ'
                        WHEN 'DOC'   THEN 'DOC'
                        WHEN 'BULTO' THEN 'BULTO'
                        ELSE              'PZ'
                    END,
                    IsActive,
                    GETUTCDATE(),
                    GETUTCDATE()
                FROM Services;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicePricingOptions");
        }
    }
}
