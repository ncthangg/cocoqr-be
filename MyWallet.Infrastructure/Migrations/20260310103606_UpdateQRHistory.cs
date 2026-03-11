using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQRHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountHolderSnapshot",
                table: "QRHistories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumberSnapshot",
                table: "QRHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCodeSnapshot",
                table: "QRHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankNameSnapshot",
                table: "QRHistories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "QRHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixedAmount",
                table: "QRHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "QRHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "QRHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "QRHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverType",
                table: "QRHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountHolderSnapshot",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "AccountNumberSnapshot",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "BankCodeSnapshot",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "BankNameSnapshot",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "IsFixedAmount",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "QRHistories");

            migrationBuilder.DropColumn(
                name: "ReceiverType",
                table: "QRHistories");
        }
    }
}
