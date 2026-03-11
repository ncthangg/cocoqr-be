using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccount : Migration
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

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "Accounts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "BankCode",
                table: "Accounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "AccountHolder",
                table: "Accounts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Accounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_IsActive",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "BankName",
                table: "Accounts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BankCode",
                table: "Accounts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountHolder",
                table: "Accounts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                column: "UserId")
                .Annotation("SqlServer:Include", new[] { "AccountNumber", "AccountHolder", "BankCode", "BankName", "AccountType", "Balance", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_IsActive",
                table: "Accounts",
                columns: new[] { "UserId", "IsActive" })
                .Annotation("SqlServer:Include", new[] { "AccountNumber", "AccountHolder", "BankCode", "BankName", "AccountType", "Balance" });
        }
    }
}
