using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCoupons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCoupons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CouponCodeSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CouponNameSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CouponDescriptionSnapshot = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BenefitTypeSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BenefitValueSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EventTypeSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCoupons_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_UserId_CouponCodeSnapshot",
                table: "UserCoupons",
                columns: new[] { "UserId", "CouponCodeSnapshot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_UserId_EventTypeSnapshot_Status",
                table: "UserCoupons",
                columns: new[] { "UserId", "EventTypeSnapshot", "Status" },
                unique: true,
                filter: "[Status] = 'CREATED'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCoupons");
        }
    }
}
