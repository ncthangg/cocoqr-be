using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBankInfo1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NapasCode",
                table: "BankInfos",
                newName: "NapasBin");

            migrationBuilder.RenameIndex(
                name: "IX_BankInfos_NapasCode",
                table: "BankInfos",
                newName: "IX_BankInfos_NapasBin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_BankInfos_NapasBin",
                table: "BankInfos",
                newName: "IX_BankInfos_NapasCode");

            migrationBuilder.RenameColumn(
                name: "NapasBin",
                table: "BankInfos",
                newName: "NapasCode");
        }
    }
}
