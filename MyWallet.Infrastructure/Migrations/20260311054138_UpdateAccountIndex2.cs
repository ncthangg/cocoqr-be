using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountIndex2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_AccountNumber_Provider",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_AccountNumber_BankCode_Provider",
                table: "Accounts",
                columns: new[] { "UserId", "AccountNumber", "BankCode", "Provider" },
                unique: true,
                filter: "[BankCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_AccountNumber_BankCode_Provider",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_AccountNumber_Provider",
                table: "Accounts",
                columns: new[] { "UserId", "AccountNumber", "Provider" },
                unique: true);
        }
    }
}
