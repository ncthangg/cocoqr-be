using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountIndex1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_AccountNumber",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_AccountNumber_Provider",
                table: "Accounts",
                columns: new[] { "UserId", "AccountNumber", "Provider" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_AccountNumber_Provider",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_AccountNumber",
                table: "Accounts",
                columns: new[] { "UserId", "AccountNumber" },
                unique: true);
        }
    }
}
