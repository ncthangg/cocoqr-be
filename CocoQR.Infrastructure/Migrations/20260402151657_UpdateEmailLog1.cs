using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocoQR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailLog1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmtpType",
                table: "EmailLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_SmtpType_CreatedAt",
                table: "EmailLogs",
                columns: new[] { "SmtpType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLogs_ToEmail_CreatedAt",
                table: "EmailLogs",
                columns: new[] { "ToEmail", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_SmtpType_CreatedAt",
                table: "EmailLogs");

            migrationBuilder.DropIndex(
                name: "IX_EmailLogs_ToEmail_CreatedAt",
                table: "EmailLogs");

            migrationBuilder.DropColumn(
                name: "SmtpType",
                table: "EmailLogs");
        }
    }
}
