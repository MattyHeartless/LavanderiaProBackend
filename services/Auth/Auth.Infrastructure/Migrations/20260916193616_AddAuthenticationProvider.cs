using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthenticationProvider",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Password");

            migrationBuilder.Sql("""
                UPDATE [u]
                SET [AuthenticationProvider] = 'Google'
                FROM [AspNetUsers] AS [u]
                WHERE NULLIF([u].[PasswordHash], '') IS NULL
                  AND EXISTS (
                    SELECT 1
                    FROM [AspNetUserLogins] AS [l]
                    WHERE [l].[UserId] = [u].[Id]
                      AND [l].[LoginProvider] = 'Google'
                  );
                """);

            migrationBuilder.Sql("""
                DELETE [l]
                FROM [AspNetUserLogins] AS [l]
                INNER JOIN [AspNetUsers] AS [u] ON [u].[Id] = [l].[UserId]
                WHERE [l].[LoginProvider] = 'Google'
                  AND [u].[AuthenticationProvider] = 'Password';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenticationProvider",
                table: "AspNetUsers");
        }
    }
}
