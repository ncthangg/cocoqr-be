using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocoQR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSmtpSetting1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SmtpSettings_ActiveUnique",
                table: "SmtpSettings");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "SmtpSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.CreateIndex(
                name: "IX_SmtpSettings_Type_Unique",
                table: "SmtpSettings",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SmtpSettings_Type_Unique",
                table: "SmtpSettings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "SmtpSettings");

            migrationBuilder.CreateIndex(
                name: "IX_SmtpSettings_ActiveUnique",
                table: "SmtpSettings",
                column: "IsActive",
                unique: true,
                filter: "[IsActive] = 1");
        }
    }
}
