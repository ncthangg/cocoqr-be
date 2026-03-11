using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_IsActive",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_IsActive",
                table: "Accounts",
                columns: new[] { "UserId", "IsActive" })
                .Annotation("SqlServer:Include", new[] { "AccountNumber", "AccountHolder", "BankCode", "BankName", "Provider", "Balance", "IsPinned", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_IsPinned_CreatedAt",
                table: "Accounts",
                columns: new[] { "UserId", "IsPinned", "CreatedAt" })
                .Annotation("SqlServer:Include", new[] { "AccountNumber", "AccountHolder", "BankCode", "BankName", "Provider", "Balance", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_IsActive",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_IsPinned_CreatedAt",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                column: "UserId")
                .Annotation("SqlServer:Include", new[] { "AccountNumber", "AccountHolder", "BankCode", "BankName", "Provider", "Balance", "IsPinned", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_IsActive",
                table: "Accounts",
                columns: new[] { "UserId", "IsActive" })
                .Annotation("SqlServer:Include", new[] { "AccountNumber", "AccountHolder", "BankCode", "BankName", "Provider", "Balance", "IsPinned" });
        }
    }
}
