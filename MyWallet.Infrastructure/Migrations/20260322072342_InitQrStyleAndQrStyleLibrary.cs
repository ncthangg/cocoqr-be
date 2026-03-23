using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitQrStyleAndQrStyleLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrImageUrl",
                table: "QRHistories");

            migrationBuilder.CreateTable(
                name: "QRStyleLibraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StyleJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRStyleLibraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QRStyleLibraries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRStyles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QrId = table.Column<long>(type: "bigint", nullable: false),
                    StyleJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRStyles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QRStyles_QRHistories_QrId",
                        column: x => x.QrId,
                        principalTable: "QRHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QRStyleLibraries_Type",
                table: "QRStyleLibraries",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_QRStyleLibraries_UserId",
                table: "QRStyleLibraries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QRStyleLibraries_UserId_IsDefault",
                table: "QRStyleLibraries",
                columns: new[] { "UserId", "IsDefault" },
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_QRStyleLibraries_UserId_Type",
                table: "QRStyleLibraries",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_QRStyles_QrId",
                table: "QRStyles",
                column: "QrId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QRStyleLibraries");

            migrationBuilder.DropTable(
                name: "QRStyles");

            migrationBuilder.AddColumn<string>(
                name: "QrImageUrl",
                table: "QRHistories",
                type: "nvarchar(MAX)",
                nullable: true);
        }
    }
}
