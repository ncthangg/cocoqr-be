using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocoQR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailLog2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailDirection",
                table: "EmailLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "OUTGOING");

            migrationBuilder.AddColumn<string>(
                name: "TemplateKey",
                table: "EmailLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_EmailDirection_CreatedAt",
                table: "EmailLogs",
                columns: new[] { "EmailDirection", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_EmailDirection_CreatedAt",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "EmailDirection",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "TemplateKey",
                table: "EmailLogs");
        }
    }
}
